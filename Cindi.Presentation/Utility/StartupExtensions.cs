using Autofac;
using Cindi.Application.Entities.Command.CreateTrackedEntity;
using Cindi.Application.Entities.Queries.GetEntities;
using Cindi.Application.Entities.Queries.GetEntity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Cindi.Presentation.Utility
{
    public static class StartupExtensions
    {
        public static void RegisterMediatr(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(GetEntitiesQueryHandler<>)).As(typeof(IRequestHandler<,>));
            builder.RegisterGeneric(typeof(GetEntityQueryHandler<>)).As(typeof(IRequestHandler<,>));
            builder.RegisterGeneric(typeof(WriteEntityCommandHandler<>)).As(typeof(IRequestHandler<,>));
            builder.RegisterAssemblyTypes(typeof(IMediator).GetTypeInfo().Assembly).AsImplementedInterfaces();

            var mediatrOpenTypes = new[]
            {
                typeof(IRequestHandler<,>),
                typeof(INotificationHandler<>),
                typeof(IPipelineBehavior<,>)
            };

            foreach (var mediatrOpenType in mediatrOpenTypes)
            {
                builder
                    .RegisterAssemblyTypes(typeof(GetEntitiesQueryHandler<>).GetTypeInfo().Assembly)
                    .AsClosedTypesOf(mediatrOpenType)
                    // when having a single class implementing several handler types
                    // this call will cause a handler to be called twice
                    // in general you should try to avoid having a class implementing for instance `IRequestHandler<,>` and `INotificationHandler<>`
                    // the other option would be to remove this call
                    // see also https://github.com/jbogard/MediatR/issues/462
                    .AsImplementedInterfaces();
            }
        }
    }
}
