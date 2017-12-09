using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChannelX.Data;

namespace ChannelX.Models.Channel
{
    public class HistoryModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime EndAt { get; set; }
        public List<String> EngagedUsersName { get; set; }
        public DateTime CreatedAt { get; set; }

    }

    public class HistoryPaginationModel
    {
        public int Total { get; set; }
        public int CurrentPage { get; set; }
        public int Count { get; set; }
    }
}
