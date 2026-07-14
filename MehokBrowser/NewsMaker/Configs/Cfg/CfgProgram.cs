using GH.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
namespace NewsMaker
{
    public class CfgProgram : CfgCore
    {
        private int _runDay = 0;
        [DataMember]
        [Display(Name = "Run Collapce", Description = "????????? ????????")]
        public bool RunCollapced { get; set; }
        //[DataMember]
        //[Display(Name = "AutoSend", Description = "????????? ?????????????")]
        //public bool AutoSending { get; set; } = true;
        [DataMember]
        [Display(Name = "Run Day", Description = "???? ???????")]
        public int RunDay
        {
            get => _runDay;
            set
            {
                _runDay = value;
            }
        }
        [DataMember]
        [Display(Name = "Run Time", Description = "????? ???????")]
        public TimeSpan RunTime { get; set; } = new TimeSpan(18, 0, 0);
        [Display(Name = "Run Date & Time", Description = "")]
        public DateTime RunDateTime
        {
            get => DateTime.Now.Date + RunTime;
            set => RunTime = value.TimeOfDay;
        }
        protected override void CreateSomething()
        {
            //????????
        }
    }
}
