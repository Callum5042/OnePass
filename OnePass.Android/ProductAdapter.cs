using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnePass.Android
{
    public class ProductAdapter : RecyclerView.Adapter
    {
        public override int ItemCount => Names.Count;

        public IList<string> Names { get; set; } = new List<string>();

        public ProductAdapter()
        {
            Names.Add("Callum");
            Names.Add("Callum 1");
            Names.Add("Callum 2");
            Names.Add("Callum 3");
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as ProductViewHolder;

            viewHolder.Name.Text = Names[position];
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.product_view, parent, false);

            var viewHolder = new ProductViewHolder(itemView);
            return viewHolder;
        }
    }

    public class ProductViewHolder : RecyclerView.ViewHolder
    {
        public TextView Name { get; private set; }

        public ProductViewHolder(View itemView) : base(itemView)
        {
            Name = itemView.FindViewById<TextView>(Resource.Id.view_name);
        }
    }
}