using GH.Configs;
using MeshokBrowser.NHibernate;
using NHibernate;
namespace GH.NHibernate
{
    public class FactoryCriatorMB : FactoryCriator<FactoryCriatorMB, User, CfgIShop>
    {
        public FactoryCriatorMB() : base()
        {
        }
        protected override void SetServerType()
        {
            _dbServerType = DbServerType.FireBird;
        }
        public override ISessionFactory GetSessionFactory()
        {
            return SessionFactoryBuilder.BuildFirebird(GetConnectionString(), typeof(User).Assembly);
        }
    }
}
