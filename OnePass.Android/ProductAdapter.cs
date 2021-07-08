using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using System;
using System.Collections.Generic;

namespace OnePass.Droid
{
    public class ProductAdapter : RecyclerView.Adapter
    {
        public override int ItemCount => Names.Count;

        public IList<string> Names { get; set; } = new List<string>();

        public event EventHandler<int> ItemClick;

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

            var viewHolder = new ProductViewHolder(itemView, OnClick);
            return viewHolder;
        }

        private void OnClick(int position)
        {
            ItemClick?.Invoke(this, position);
        }

        private class ProductViewHolder : RecyclerView.ViewHolder
        {
            public TextView Name { get; private set; }

            public ProductViewHolder(View itemView, Action<int> listener) : base(itemView)
            {
                Name = itemView.FindViewById<TextView>(Resource.Id.view_name);

                itemView.Click += (sender, args) =>
                {
                    listener(LayoutPosition);
                };
            }
        }
    }
}