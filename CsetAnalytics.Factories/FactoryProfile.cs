﻿using System;
using System.Collections.Generic;
using AutoMapper;
using CsetAnalytics.DomainModels.Models;
using CsetAnalytics.ViewModels;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace CsetAnalytics.Factories
{
    public class FactoryProfile : Profile
    {
        public FactoryProfile()
        {
            RegisterMappings();
        }

        private void RegisterMappings()
        {
            CreateMap<AnalyticDemographic, AnalyticDemographicViewModel>();
            CreateMap<AnalyticQuestionAnswer, AnalyticQuestionViewModel>();
            CreateMap<AnalyticDemographicViewModel, AnalyticDemographic>();
            CreateMap<AnalyticQuestionViewModel, AnalyticQuestionAnswer>();
            CreateMap<AnalyticAssessmentViewModel, Assessment>();
                           
        }
    }
}
