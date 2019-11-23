using System;
using System.Collections.Generic;

using Cultural_Data_Visualisation_on_the_map.Core.Models;

namespace Cultural_Data_Visualisation_on_the_map.WebApi.Models
{
    // TODO WTS: Replace or update this interface when no longer using sample data.
    public interface IItemRepository
    {
        void Add(SampleCompany item);

        void Update(SampleCompany item);

        SampleCompany Remove(string id);

        SampleCompany Get(string id);

        IEnumerable<SampleCompany> GetAll();
    }
}
