using AutoMapper;
using BlazeJump.Common.Models;

namespace BlazeJump.Common.Mappers
{
	public class NEventProfile : Profile
	{
		public NEventProfile()
		{
			CreateMap<NEvent, NEvent>();
			CreateMap<SignableNEvent, NEvent>();
			CreateMap<NEvent, SignableNEvent>();
		}
	}
}
