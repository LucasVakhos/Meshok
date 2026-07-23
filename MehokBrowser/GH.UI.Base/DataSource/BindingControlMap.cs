using DevExpress.XtraEditors;

namespace MehokBrowser.Controls
{
    /// <summary>Карта привязки контрола к DataSource.</summary>
    internal struct BindingControlMap
    {
        public readonly BaseEdit Control;
        public readonly bool ReadOnly;

        public BindingControlMap(BaseEdit edit)
        {
            Control = edit;
            ReadOnly = edit.Properties.ReadOnly;
        }
    }
}
