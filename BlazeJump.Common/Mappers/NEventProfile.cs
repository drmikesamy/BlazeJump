using AutoMapper;
using BlazeJump.Common.Models;

namespace BlazeJump.Common.Mappers
{
	public class NEventProfile : Profile
	{
		public NEventProfile()
		{
			CreateMap<NEvent, NEvent>();
			CreateMap<EventTag, EventTag>();
			CreateMap<NEvent, SignableNEvent>()
				.ForMember(x => x.Id, opt => opt.Ignore())
				.ReverseMap();
		}
	}
}
