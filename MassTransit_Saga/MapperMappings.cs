using AutoMapper;
using MassTransit_Saga.Contracts;

namespace MassTransit_Saga
{
    public class MapperMappings
    {
        public static void Map()
        {
            Mapper.Initialize(c =>
            {
                c.CreateMap<BookCreated, Book>();
                c.ForAllMaps((map, exp) => exp.ForAllOtherMembers(opt => opt.Ignore()));
            });
        }
    }
}
