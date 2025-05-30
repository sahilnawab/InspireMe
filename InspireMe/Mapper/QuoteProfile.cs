using AutoMapper;
using InspireMe.Models;

namespace InspireMe.Mapper
{
    public class QuoteProfile : Profile
    {
        public QuoteProfile()
        {
            CreateMap<Quote, QuoteModel>()
                .ForMember(dest=>dest.CreatedByName,opt=>opt.MapFrom(src=>src.User.Name))
                .ForMember(dest => dest.ImagePath, opt => opt.MapFrom(src => src.ImageUrl)).ReverseMap();


        }
    }

}
