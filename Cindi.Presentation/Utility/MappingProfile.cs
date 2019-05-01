using AutoMapper;
using Cindi.Domain.Entities.GlobalValues;
using Cindi.Domain.Entities.Users;
using Cindi.Presentation.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cindi.Presentation.Utility
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserVM>();
            CreateMap<GlobalValue, GlobalValueVM>();
        }
    }
}
