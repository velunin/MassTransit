namespace MassTransit.Registration
{
    using Courier;


    public class CompensateActivityEndpointRegistrationConfigurator<TActivity, TLog> :
        EndpointRegistrationConfigurator<ICompensateActivity<TLog>>,
        ICompensateActivityEndpointRegistrationConfigurator<TActivity, TLog>
        where TActivity : class, ICompensateActivity<TLog>
        where TLog : class
    {
    }
}
