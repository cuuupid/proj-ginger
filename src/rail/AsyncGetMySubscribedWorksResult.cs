using System.Collections.Generic;

namespace rail
{
	public class AsyncGetMySubscribedWorksResult : EventBase
	{
		public uint total_available_works;

		public List<RailSpaceWorkDescriptor> spacework_descriptors = new List<RailSpaceWorkDescriptor>();
	}
}
