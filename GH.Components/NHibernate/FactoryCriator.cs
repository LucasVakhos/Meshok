using NHibernate;
namespace GH.Components
{
    public class FactoryCriator<TFactoryCriator, TUser, TSetting> : IFactoryCriator
        where TFactoryCriator : IFactoryCriator
        where TUser : BaseUser
        where TSetting : CfgCoreConnection
    {
        protected DbServerType _dbServerType;
    public DbServerType DbServerType => _dbServerType;
    public FactoryCriator()
        {
            SetServerType();
            NHHelper.SetVerasticallyFactoryCriator(this);
        }
    protected virtual void SetServerType()
        {
            throw new NotImplemented(nameof(SetServerType), this);
        }
    public virtual ISessionFactory GetSessionFactory()
        {
            try
            {
                throw new NotImplemented(nameof(GetSessionFactory), this);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return SessionFactoryBuilder.BuildMySql(GetConnectionString(), typeof(TUser).Assembly);
            }
        }
        //protected virtual CfgCoreConnection GetCfg()
        //{
        //    throw new NotImplemented(nameof(GetCfg), this);
        //    //пример return IniHelper.Cfg<TSetting>();
        //}
    public string GetConnectionString()
        {
            try
            {
                //return GetCfg().ConnectionString();
                return IniHelper.Cfg<TSetting>().ConnectionString();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return string.Empty;
            }
        }
    }
}
