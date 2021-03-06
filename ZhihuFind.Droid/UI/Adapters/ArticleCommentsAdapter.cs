using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using FFImageLoading.Transformations;
using FFImageLoading.Views;
using FFImageLoading.Work;
using System;
using System.Collections.Generic;
using ZhihuFind.Droid.UI.Listeners;
using ZhihuFind.Droid.Utils;
using ZhihuFind.Droid.ViewModel;

namespace ZhihuFind.Droid.UI.Adapters
{
    public class ArticleCommentsAdapter : RecyclerView.Adapter
    {
        public const int LoadingView = 0x00000111;
        public const int FooterView = 0x00000222;
        private Context context;
        protected LayoutInflater layoutInflater;
        private LinearLayout footerLayout;
        private LinearLayout copyFooterLayout;
        private View loadMoreFailedView;

        private bool loadingMoreEnable;

        public List<ArticleCommentModel> List;
        public IOnLoadMoreListener OnLoadMoreListener;

        public ArticleCommentsAdapter()
        {
            List = new List<ArticleCommentModel>();
        }
        public override int ItemCount
        {
            get
            {
                var count = 0;
                if (List.Count > 0)
                {
                    count = List.Count + 1;
                }
                else
                {
                    if (footerLayout != null)
                    {
                        count = 1;
                    }
                }
                return count;
            }
        }
        public override int GetItemViewType(int position)
        {
            if (List.Count == 0 || position == List.Count)
            {
                if (footerLayout == null)
                {
                    return LoadingView;
                }
                else
                {
                    return FooterView;
                }
            }
            return base.GetItemViewType(position);
        }

        public override async void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            int viewType = viewHolder.ItemViewType;

            switch (viewType)
            {
                case LoadingView:
                    AddLoadMore(viewHolder);
                    break;
                case FooterView:
                    break;
                default:
                    var item = (ItemViewHolder)viewHolder;
                    var count = List.Count;
                    var model = List[position];

                    item.author.Text = model.author.Name;
                    item.content.Text = model.content;
                    item.createAt.Text = DateTimeUtils.CommonTime(Convert.ToDateTime(model.createdTime));
                    if (model.likesCount > 0)
                        item.upCount.Text = " " + model.likesCount.ToString();
                    var avatar = model.author.Avatar.Template.Replace("{id}", model.author.Avatar.Id);
                    avatar = avatar.Replace("{size}", "s");
                    await ImageService.Instance.LoadUrl(avatar)
                          .Retry(3, 200)
                          .DownSample(40, 40)
                          .Transform(new CircleTransformation())
                          .LoadingPlaceholder("ic_placeholder.png", ImageSource.ApplicationBundle)
                          .ErrorPlaceholder("ic_placeholder.png", ImageSource.ApplicationBundle)
                          .IntoAsync(item.avatar);
                    break;
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            this.context = parent.Context;
            this.layoutInflater = LayoutInflater.From(context);
            switch (viewType)
            {
                case LoadingView:
                    return new LoadingViewHolder(layoutInflater.Inflate(Resource.Layout.recyclerview_loadmore, parent, false));
                case FooterView:
                    return new FooterViewHolder(footerLayout);
                default:
                    return new ItemViewHolder(layoutInflater.Inflate(Resource.Layout.comment_item, parent, false));
            }
        }
        public class ItemViewHolder : RecyclerView.ViewHolder
        {
            public TextView author { get; set; }
            public ImageViewAsync avatar { get; set; }
            public TextView createAt { get; set; }
            public TextView content { get; set; }
            public TextView upCount { get; set; }
            public ItemViewHolder(View view)
                : base(view)
            {
                author = view.FindViewById<TextView>(Resource.Id.author);
                avatar = view.FindViewById<ImageViewAsync>(Resource.Id.avatar);
                createAt = view.FindViewById<TextView>(Resource.Id.createAt);
                content = view.FindViewById<TextView>(Resource.Id.content);
                upCount = view.FindViewById<TextView>(Resource.Id.upCount);
            }
        }
        public class LoadingViewHolder : RecyclerView.ViewHolder
        {
            public LoadingViewHolder(View view)
                : base(view)
            {
            }
        }
        public class FooterViewHolder : RecyclerView.ViewHolder
        {
            public FooterViewHolder(View view)
                : base(view)
            {
            }
        }
        public void NewData(List<ArticleCommentModel> list)
        {
            this.List = list;
            if (loadMoreFailedView != null)
            {
                RemoveFooterView(loadMoreFailedView);
            }
            NotifyDataSetChanged();
        }
        public void Remove(int position)
        {
            List.RemoveAt(position);
            NotifyItemRemoved(position);
        }
        public void Add(int position, ArticleCommentModel item)
        {
            List.Insert(position, item);
            NotifyItemInserted(position);
        }
        public void AddData(List<ArticleCommentModel> newList)
        {
            loadingMoreEnable = false;
            this.List.AddRange(newList);
            NotifyItemRangeChanged(List.Count - newList.Count, newList.Count);
        }
        public void AddFooterView(View footer)
        {
            AddFooterView(footer, -1);
        }
        public void AddFooterView(View footer, int index)
        {
            if (footerLayout == null)
            {
                if (copyFooterLayout == null)
                {
                    footerLayout = new LinearLayout(footer.Context);
                    footerLayout.Orientation = Orientation.Vertical;
                    footerLayout.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
                    copyFooterLayout = footerLayout;
                }
                else
                {
                    footerLayout = copyFooterLayout;
                }
            }
            index = index >= footerLayout.ChildCount ? -1 : index;
            footerLayout.AddView(footer, index);
            this.NotifyItemChanged(ItemCount);
        }
        public void RemoveFooterView(View footer)
        {
            if (footerLayout == null) return;

            footerLayout.RemoveView(footer);
            if (footerLayout.ChildCount == 0)
            {
                footerLayout = null;
            }
            this.NotifyDataSetChanged();
        }
        public void RemoveAllFooterView()
        {
            if (footerLayout == null) return;

            footerLayout.RemoveAllViews();
            footerLayout = null;
        }
        public void ShowLoadMoreFailedView()
        {
            LoadComplete();
            if (loadMoreFailedView == null)
            {
                loadMoreFailedView = layoutInflater.Inflate(Resource.Layout.recyclerview_loadmore_failed, null);
                loadMoreFailedView.Click += delegate
                {
                    RemoveFooterView(loadMoreFailedView);
                };
            }
            AddFooterView(loadMoreFailedView);
        }
        public void LoadComplete()
        {
            loadingMoreEnable = false;
            this.NotifyItemChanged(ItemCount);
        }

        private void AddLoadMore(RecyclerView.ViewHolder holder)
        {
            if (!loadingMoreEnable)
            {
                loadingMoreEnable = true;
                OnLoadMoreListener.OnLoadMoreRequested();
            }
        }
    }
}