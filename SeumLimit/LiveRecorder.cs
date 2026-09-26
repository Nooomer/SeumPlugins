using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using Rewired;
using UnityEngine;
using IOPath = System.IO.Path;

namespace SeumLimit
{
    /// <summary>
    /// Records a run as it is played - not as a replay - one row per physics tick, with everything
    /// a replay leaves out: the input (every Rewired action, held and pressed since the previous
    /// tick), the character's real velocity and grounded state, and every simple field of
    /// CharacterMotor and FPSInputController, which is where roar charge, teleport state, speed
    /// boosts and the like live. This is the data the powerup model gets fitted to.
    ///
    /// Rows are taken in a postfix on <c>GameManager.FixedUpdate</c>, the method that runs the
    /// character's physics step and records the replay frame, so row n lines up with replay frame
    /// n (column replayFrame).
    ///
    /// A file is written to BepInEx/cache/SeumLimit/live when the run ends: finish, death or
    /// restart.
    /// </summary>
    internal static class LiveRecorder
    {
        private const int MaxTicks = 60 * 300;

        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private struct Column
        {
            internal string Name;
            internal Func<object> Owner;
            internal FieldInfo Field;
            internal int Component; // -1 scalar, 0..2 vector component
        }

        private static FPSInputController boundInput;
        private static List<Column> columns;
        private static IList<InputAction> actions;
        private static bool[] pressedSince;
        private static bool[] releasedSince;
        private static int framesSinceTick;

        private static readonly List<string> rows = new List<string>();
        private static GameManager recordingManager;
        private static int level;
        private static int mutator;
        private static float finishTime = -1f;
        private static float finishOffset;

        private static FieldInfo levelSpawnTime;
        private static FieldInfo levelFinishTimeOffset;

        internal static void Apply(Harmony harmony)
        {
            harmony.Patch(AccessTools.Method(typeof(GameManager), "FixedUpdate"),
                postfix: new HarmonyMethod(typeof(LiveRecorder), nameof(FixedUpdatePostfix)));
            harmony.Patch(AccessTools.Method(typeof(GameManager), "finishLevel"),
                prefix: new HarmonyMethod(typeof(LiveRecorder), nameof(FinishLevelPrefix)));
            levelSpawnTime = AccessTools.Field(typeof(GameManager), "levelSpawnTime");
            levelFinishTimeOffset = AccessTools.Field(typeof(GameManager), "levelFinishTimeOffset");
        }

        /// <summary>Called every rendered frame: catches presses shorter than a physics tick.</summary>
        internal static void PollInput()
        {
            if (boundInput == null || boundInput.player == null || actions == null)
            {
                return;
            }

            framesSinceTick++;
            for (int i = 0; i < actions.Count; i++)
            {
                if (boundInput.player.GetButtonDown(actions[i].id))
                {
                    pressedSince[i] = true;
                }

                if (boundInput.player.GetButtonUp(actions[i].id))
                {
                    releasedSince[i] = true;
                }
            }
        }

        private static void FixedUpdatePostfix(GameManager __instance)
        {
            try
            {
                Tick(__instance);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Live recorder threw, dropping this run: " + e);
                rows.Clear();
                recordingManager = null;
            }
        }

        /// <summary>
        /// Leaderboard time of the last finish, in ms, and how many finishes there have been.
        /// finishLevel turns calculateCurrentLevelTime() * 1000 into the score first thing, with
        /// the sub-tick offset already set, so reading it on entry gives exactly the number the
        /// game shows and uploads.
        /// </summary>
        internal static int LastFinishMs = -1;
        internal static int FinishCount;

        /// <summary>
        /// The session that finished. Taken here because nothing else keeps it for us:
        /// Replay.lastFinishedReplay is only filled when the replay viewer is opened.
        /// </summary>
        internal static Replay.ReplaySession LastFinishSession;

        private static void FinishLevelPrefix(GameManager __instance)
        {
            // finishLevel also runs for replays reaching the exit (the one behind the win screen
            // loops); only a run that is being recorded is a real finish.
            if (!Replay.recording)
            {
                return;
            }

            try
            {
                LastFinishMs = (int)(__instance.calculateCurrentLevelTime() * 1000f);
                LastFinishSession = Replay.replay;
                FinishCount++;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Could not read the finish time: " + e.Message);
            }
        }

        private static void Tick(GameManager manager)
        {
            if (!LimitConfig.RecordLive.Value)
            {
                return;
            }

            bool running = manager.gameplayState == GameManager.GameplayState.IN_GAME && GameManager.firstInputReceived;

            if (recordingManager != null && (recordingManager != manager || !running))
            {
                // By now the level may already be reset, so nothing from this tick is recorded:
                // the finishing tick was the last row, and its time was taken there.
                Flush(recordingManager);
            }

            if (!running)
            {
                return;
            }

            if (recordingManager == null)
            {
                // The dump shows GameManager on the character or one of its parents.
                FPSInputController input = manager.GetComponentInChildren<FPSInputController>();
                if (input == null)
                {
                    return;
                }

                Bind(input);
                recordingManager = manager;
                level = Game.currentLevel;
                mutator = LevelSelector.currentLevelMutator;
                finishTime = -1f;
                rows.Clear();
            }

            if (rows.Count < MaxTicks)
            {
                AddRow(manager);
            }
        }

        private static void Bind(FPSInputController input)
        {
            if (boundInput == input && columns != null)
            {
                return;
            }

            boundInput = input;
            CharacterMotor motor = input.GetComponent<CharacterMotor>();

            columns = new List<Column>();
            AddColumns("motor.", () => motor, typeof(CharacterMotor));
            AddColumns("movement.", () => motor.movement, typeof(CharacterMotor.CharacterMotorMovement));
            AddColumns("jumping.", () => motor.jumping, typeof(CharacterMotor.CharacterMotorJumping));
            AddColumns("sliding.", () => motor.sliding, typeof(CharacterMotor.CharacterMotorSliding));
            AddColumns("input.", () => input, typeof(FPSInputController));

            actions = new List<InputAction>();
            foreach (InputAction action in ReInput.mapping.Actions)
            {
                actions.Add(action);
            }

            pressedSince = new bool[actions.Count];
            releasedSince = new bool[actions.Count];
        }

        private static void AddColumns(string prefix, Func<object> owner, Type type)
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (FieldInfo field in type.GetFields(flags))
            {
                if (field.IsLiteral)
                {
                    continue;
                }

                Type t = field.FieldType;
                if (t == typeof(Vector3))
                {
                    for (int c = 0; c < 3; c++)
                    {
                        columns.Add(new Column { Name = prefix + field.Name + "." + "xyz"[c], Owner = owner, Field = field, Component = c });
                    }
                }
                else if (t.IsPrimitive || t.IsEnum)
                {
                    columns.Add(new Column { Name = prefix + field.Name, Owner = owner, Field = field, Component = -1 });
                }
            }
        }

        private static string Header()
        {
            StringBuilder sb = new StringBuilder("row,replayFrame,fixedTime,levelTime,renderFrames,x,y,z,orb,orb.x,orb.y,orb.z,orb.vx,orb.vy,orb.vz");
            for (int i = 0; i < actions.Count; i++)
            {
                string name = actions[i].name.Replace(',', '_').Replace(' ', '_');
                sb.Append(",held.").Append(name).Append(",down.").Append(name).Append(",up.").Append(name);
            }

            for (int i = 0; i < 4; i++)
            {
                sb.Append(",axis").Append(i.ToString(Inv));
            }

            foreach (Column c in columns)
            {
                sb.Append(',').Append(c.Name);
            }

            return sb.ToString();
        }

        private static void AddRow(GameManager manager)
        {
            FPSInputController input = boundInput;
            StringBuilder sb = new StringBuilder(4096);

            int replayFrame = Replay.recording && Replay.replay != null ? Replay.replay.frameCount - 1 : -1;

            // The replay stops recording inside the tick that reaches the exit, while the state is
            // still IN_GAME: that is the moment the level time is final.
            if (replayFrame < 0 && rows.Count > 0 && finishTime < 0f)
            {
                finishTime = LastFinishMs / 1000f;
                finishOffset = levelFinishTimeOffset == null ? 0f : (float)levelFinishTimeOffset.GetValue(manager);
            }
            float spawn = levelSpawnTime == null ? 0f : (float)levelSpawnTime.GetValue(manager);

            sb.Append(rows.Count.ToString(Inv)).Append(',')
                .Append(replayFrame.ToString(Inv)).Append(',')
                .Append(Time.fixedTime.ToString("R", Inv)).Append(',')
                .Append((Time.fixedTime - spawn).ToString("R", Inv)).Append(',')
                .Append(framesSinceTick.ToString(Inv));

            Vector3 p = input.transform.position;
            Append(sb, p);

            Projectile orb = input.activeTeleportProjectile;
            if (orb != null)
            {
                sb.Append(",1");
                Append(sb, orb.transform.position);
                Append(sb, orb.velocity);
            }
            else
            {
                sb.Append(",0,,,,,,");
            }

            Player player = input.player;
            for (int i = 0; i < actions.Count; i++)
            {
                bool held = player != null && player.GetButton(actions[i].id);
                sb.Append(held ? ",1" : ",0")
                    .Append(pressedSince[i] ? ",1" : ",0")
                    .Append(releasedSince[i] ? ",1" : ",0");
                pressedSince[i] = false;
                releasedSince[i] = false;
            }

            for (int i = 0; i < 4; i++)
            {
                sb.Append(',').Append(player == null ? "" : player.GetAxis(i).ToString("R", Inv));
            }

            foreach (Column c in columns)
            {
                sb.Append(',');
                object owner = c.Field.IsStatic ? null : c.Owner();
                if (!c.Field.IsStatic && owner == null)
                {
                    continue;
                }

                object value = c.Field.GetValue(owner);
                if (c.Component >= 0)
                {
                    sb.Append(((Vector3)value)[c.Component].ToString("R", Inv));
                }
                else if (value is float)
                {
                    sb.Append(((float)value).ToString("R", Inv));
                }
                else if (value is bool)
                {
                    sb.Append((bool)value ? '1' : '0');
                }
                else
                {
                    sb.Append(Convert.ToString(value is Enum ? (object)Convert.ToInt32(value, Inv) : value, Inv));
                }
            }

            framesSinceTick = 0;
            rows.Add(sb.ToString());
        }

        private static void Append(StringBuilder sb, Vector3 v)
        {
            sb.Append(',').Append(v.x.ToString("R", Inv))
                .Append(',').Append(v.y.ToString("R", Inv))
                .Append(',').Append(v.z.ToString("R", Inv));
        }

        private static void Flush(GameManager manager)
        {
            GameManager.GameplayState state = manager.gameplayState;
            recordingManager = null;
            if (rows.Count < 2)
            {
                rows.Clear();
                return;
            }

            try
            {
                string outcome = finishTime >= 0f && state == GameManager.GameplayState.FINISH_LEVEL
                    ? "FINISH_" + Mathf.RoundToInt(finishTime * 1000f).ToString(Inv)
                        + "ms_offset" + (finishOffset * 1000f).ToString("0.0", Inv)
                    : state.ToString();

                string dir = IOPath.Combine(Paths.CachePath, IOPath.Combine("SeumLimit", "live"));
                Directory.CreateDirectory(dir);
                string name = DateTime.Now.ToString("yyyyMMdd_HHmmss", Inv) + "_L" + level.ToString(Inv)
                    + "_m" + mutator.ToString(Inv) + "_" + outcome + ".csv";

                StringBuilder file = new StringBuilder();
                file.AppendLine(Header());
                foreach (string row in rows)
                {
                    file.AppendLine(row);
                }

                File.WriteAllText(IOPath.Combine(dir, name), file.ToString(), Encoding.UTF8);
                Plugin.Log.LogInfo("Live run recorded: " + name + " (" + rows.Count + " ticks)");
            }
            finally
            {
                rows.Clear();
            }
        }
    }
}
