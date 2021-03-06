using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace ZhihuFind.Droid.ViewModel
{
    public class DailyModel
    {
        public string body { get; set; }
        public string image_source { get; set; }
        public string title { get; set; }
        public string image { get; set; }
        public string share_url { get; set; }
        public string ga_prefix { get; set; }
        public int type { get; set; }
        public int id { get; set; }
        public List<string> js { get; set; }
        public List<string> css { get; set; }
        public DateTime updatetime { get; set; }
    }
}