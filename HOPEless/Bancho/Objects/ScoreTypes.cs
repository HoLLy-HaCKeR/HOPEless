﻿using System.Collections.Generic;
using System.Linq;
using HOPEless.Extensions;
using HOPEless.osu;
using osu.Shared.Serialization;

namespace HOPEless.Bancho.Objects
{
    public class BanchoScoreFrame : ISerializable
    {
        public int Time;
        public byte Id;
        public ushort Count300;
        public ushort Count100;
        public ushort Count50;
        public ushort CountGeki;
        public ushort CountKatu;
        public ushort CountMiss;
        public int TotalScore;
        public ushort MaxCombo;
        public ushort CurrentCombo;
        public bool Perfect;
        public int CurrentHp;
        public int TagByte;
        public bool UsingScoreV2;
        public double ComboPortion;
        public double BonusPortion;

        public BanchoScoreFrame() { }
        public BanchoScoreFrame(byte[] data) => this.Populate(data);

        public void ReadFromStream(SerializationReader r)
        {
            Time = r.ReadInt32();
            Id = r.ReadByte();
            Count300 = r.ReadUInt16();
            Count100 = r.ReadUInt16();
            Count50 = r.ReadUInt16();
            CountGeki = r.ReadUInt16();
            CountKatu = r.ReadUInt16();
            CountMiss = r.ReadUInt16();
            TotalScore = r.ReadInt32();
            MaxCombo = r.ReadUInt16();
            CurrentCombo = r.ReadUInt16();
            Perfect = r.ReadBoolean();
            CurrentHp = r.ReadByte();
            TagByte = r.ReadByte();
            UsingScoreV2 = r.ReadBoolean();
            ComboPortion = UsingScoreV2 ? r.ReadDouble() : 0;
            BonusPortion = UsingScoreV2 ? r.ReadDouble() : 0;
        }

        public void WriteToStream(SerializationWriter w)
        {
            w.Write(Time);
            w.Write(Id);
            w.Write(Count300);
            w.Write(Count100);
            w.Write(Count50);
            w.Write(CountGeki);
            w.Write(CountKatu);
            w.Write(CountMiss);
            w.Write(TotalScore);
            w.Write(MaxCombo);
            w.Write(CurrentCombo);
            w.Write(Perfect);
            w.Write((byte)CurrentHp);
            w.Write((byte)TagByte);
            w.Write(UsingScoreV2);
            if (UsingScoreV2) {
                w.Write(ComboPortion);
                w.Write(BonusPortion);
            }
        }
    }

    public class BanchoReplayFrame : ISerializable
    {
        public float MouseX;
        public float MouseY;
        public ButtonState ButtonState;
        public int Time;

        public BanchoReplayFrame() { }
        public BanchoReplayFrame(byte[] data) => this.Populate(data);

        public BanchoReplayFrame(ButtonState buttons, float mx, float my, int time)
        {
            ButtonState = buttons;
            MouseX = mx;
            MouseY = my;
            Time = time;
        }

        public void ReadFromStream(SerializationReader r)
        {
            ButtonState = (ButtonState)r.ReadByte();
            r.ReadByte();  //unused byte, always 0?
            MouseX = r.ReadSingle();
            MouseY = r.ReadSingle();
            Time = r.ReadInt32();
        }

        public void WriteToStream(SerializationWriter w)
        {
            w.Write((byte)ButtonState);
            w.Write((byte)0);
            w.Write(MouseX);
            w.Write(MouseY);
            w.Write(Time);
        }
    }

    public class BanchoReplayFrameBundle : ISerializable
    {
        public List<BanchoReplayFrame> ReplayFrames;
        public BanchoScoreFrame CurrentScoreState;
        public ReplayAction Action;

        /// <summary> 
        /// Could be either spectator ID (if <seealso cref="Action"/> is 
        /// <seealso cref="ReplayAction.WatchingOther"/>) or RNG seed (otherwise).
        /// </summary>
        public int ExtraData;

        public BanchoReplayFrameBundle() { }
        public BanchoReplayFrameBundle(byte[] data) => this.Populate(data);

        public BanchoReplayFrameBundle(ReplayAction action, BanchoScoreFrame scoreFrame,
            IEnumerable<BanchoReplayFrame> replayFrames, int extra = 0)
        {
            Action = action;
            CurrentScoreState = scoreFrame;
            ReplayFrames = replayFrames.ToList();
            ExtraData = extra;
        }

        public void ReadFromStream(SerializationReader r)
        {
            ExtraData = r.ReadInt32();

            ReplayFrames = new List<BanchoReplayFrame>();
            int length = r.ReadUInt16();
            for (int i = 0; i < length; i++) {
                var f = new BanchoReplayFrame();
                f.ReadFromStream(r);
                ReplayFrames.Add(f);
            }

            Action = (ReplayAction)r.ReadByte();

            (CurrentScoreState = new BanchoScoreFrame()).ReadFromStream(r);
        }

        public void WriteToStream(SerializationWriter w)
        {
            w.Write(ExtraData);
            w.Write((ushort)ReplayFrames.Count);
            foreach (BanchoReplayFrame f in ReplayFrames) f.WriteToStream(w);
            w.Write((byte)Action);
            CurrentScoreState.WriteToStream(w);
        }
    }
}
