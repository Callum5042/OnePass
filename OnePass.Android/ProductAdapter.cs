using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using OnePass.Models;
using System;
using System.Collections.Generic;

namespace OnePass.Droid
{
    public class ProductAdapter : RecyclerView.Adapter
    {
        public override int ItemCount => Accounts.Count;

        public IList<Account> Accounts { get; set; } = new List<Account>();

        public event EventHandler<int> ItemClick;

        public ProductAdapter(IList<Account> accounts)
        {
            Accounts = accounts;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var viewHolder = holder as ProductViewHolder;

            //viewHolder.Name.Text = Accounts[position].Name;
            //viewHolder.Login.Text = Accounts[position].Login;
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

            public TextView Login { get; private set; }

            public ProductViewHolder(View itemView, Action<int> listener) : base(itemView)
            {
                Name = itemView.FindViewById<TextView>(Resource.Id.view_name);
                Login = itemView.FindViewById<TextView>(Resource.Id.view_login);

                itemView.Click += (sender, args) =>
                {
                    listener(LayoutPosition);
                };
            }
        }
    }
}