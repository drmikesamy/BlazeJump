using AutoMapper;
using BlazeJump.Client.Models;

namespace BlazeJump.Client.Mappers
{
	public class NEventProfile : Profile
	{
		public NEventProfile() {
			CreateMap<NEvent, NEvent>();
		}
	}
}
