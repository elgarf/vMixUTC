using System;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Runtime.Serialization; // <-- ����� �������� ���� using

namespace vMixController.Classes
{
    /// <summary>
    /// ��������������� enum � ������� ��� ���� ������.
    /// ������� Flags ��������� ��������� �������� � �������� ���������� (��������, .HasFlag()).
    /// </summary>
    [Flags]
    [Serializable] // ���� �������� enum ��� ������������� ��� �������
    public enum DaysOfWeek
    {
        None = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,
        Everyday = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday
    }


    /// <summary>
    /// ����� ��� �������� ���������� � �������.
    /// ������������ ����������� �� ��������� ������� ��� MVVM � �������� ������������.
    /// </summary>
    [Serializable]
    public class ScheduledEvent : ObservableObject, IComparable<ScheduledEvent>, ISerializable
    {
        private DateTime _timeOfDay;
        public DateTime TimeOfDay
        {
            get => _timeOfDay;
            set => SetProperty(ref _timeOfDay, value);
        }

        [NonSerialized]
        private string _command;
        public string Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        [NonSerialized]
        private DaysOfWeek _days;
        public DaysOfWeek Days
        {
            get => _days;
            set => SetProperty(ref _days, value);
        }

        /// <summary>
        /// ��������� ����������� ��� ���������� ��������� ���
        /// �������� ����� ����������� � UI � ��� ��������� ����� ������������.
        /// </summary>
        public ScheduledEvent() { }


        #region ISerializable Implementation

        /// <summary>
        /// ����������� �����������, ������������ ��� ��������������.
        /// �� ���������, ������ ��� ������� ����� ObservableObject �� �������� �������������.
        /// </summary>
        protected ScheduledEvent(SerializationInfo info, StreamingContext context)
        {
            // ��������������� ��������� ������� �� ������
            TimeOfDay = (DateTime)info.GetValue("TimeOfDay", typeof(DateTime));
            Command = info.GetString("Command");
            Days = (DaysOfWeek)info.GetValue("Days", typeof(DaysOfWeek));
        }

        /// <summary>
        /// ����� ��� ������������ �������.
        /// �� ���� ���������, ����� ������ ����� ���������.
        /// </summary>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("TimeOfDay", TimeOfDay);
            info.AddValue("Command", Command);
            info.AddValue("Days", Days);
        }

        #endregion


        /// <summary>
        /// ��� ������� ���������� �� �������
        /// </summary>
        public int CompareTo(ScheduledEvent other)
        {
            if (other == null) return 1;
            return TimeOfDay.CompareTo(other.TimeOfDay);
        }
    }
}

