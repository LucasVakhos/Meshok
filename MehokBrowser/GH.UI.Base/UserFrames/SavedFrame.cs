using System.ComponentModel;
using MehokBrowser.UI.Interfaces;
using RunContext = MehokBrowser.Application.RunContext;
using GHPropertyAttribute = LB.Libs.GHPropertyAttribute;
using ICaption = MehokBrowser.UI.Interfaces.ICaption;
using ISavedControl = MehokBrowser.UI.Interfaces.ISavedControl;
using IOpenData = MehokBrowser.UI.Interfaces.IOpenData;
using MehokBrowser.Application;
namespace MehokBrowser.Frames.Base
{
    public class SavedFrame : AbstractFrame, ICaption, ISavedControl, IOpenData
    {
        Control _activeControl = null;
        private bool _saveLayout = true;
        [GHProperty, DefaultValue(true)]
        public bool SaveLayout { get => _saveLayout; set => _saveLayout = value; }
        new public Control Parent
        {
            get => base.Parent;
            set
            {
                if (base.Parent != null)
                {
                    _activeControl = ActiveControl;
                    SaveControls();
                    CloseData();
                }
                base.Parent = value;
                if (value != null)
                {
                    if (RunContext.AppRunning)
                    {
                        LoadControls();
                        System.Windows.Forms.Application.DoEvents();
                        if (_activeControl != null)
                            ActiveControl = _activeControl;
                        OpenData();
                    }
                }
            }
        }
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
        }

        public SavedFrame()
        {
        }
        public virtual void LoadControls()
        {
            if (!_saveLayout)
                return;
            foreach (var item in Controls)
            {
                if (item is ISavedControl saved)
                    saved.LoadControls();
            }
        }
        public virtual void SaveControls()
        {
            if (!_saveLayout)
                return;
            foreach (var item in Controls)
            {
                if (item is ISavedControl saved)
                    saved.SaveControls();
            }
        }
    }
}
