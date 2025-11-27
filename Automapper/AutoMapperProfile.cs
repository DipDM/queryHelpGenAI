using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using queryHelp.DTOs;
using queryHelp.Models;

namespace queryHelp.Automapper
{

    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserReadDto>();
            CreateMap<UserCreateDto, User>();
            CreateMap<UserUpdateDto, User>();


            CreateMap<DataSource, DataSourceReadDto>();
            CreateMap<DataSourceCreateDto, DataSource>();
            CreateMap<DataSourceUpdateDto, DataSource>();


            CreateMap<QueryTemplate, QueryTemplateReadDto>();
            CreateMap<QueryTemplateCreateDto, QueryTemplate>();
            CreateMap<QueryTemplateUpdateDto, QueryTemplate>();


            CreateMap<SavedQuery, SavedQueryReadDto>();
            CreateMap<SavedQueryCreateDto, SavedQuery>();
            CreateMap<SavedQueryUpdateDto, SavedQuery>();


            CreateMap<QueryRun, QueryRunReadDto>();
        }
    }
}
