using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SparrowCloud.Services
{
    public abstract class ServiceBase
    {
        private readonly ILogger<ServiceBase> _logger;

        protected readonly IServiceProvider _serviceProvider;

        public ServiceBase(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<ServiceBase>>();

            _serviceProvider = serviceProvider;
        }
    }
}
