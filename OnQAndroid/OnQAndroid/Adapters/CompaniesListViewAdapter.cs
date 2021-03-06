using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using SQLite;
using System;
using Firebase.Xamarin.Database;
using OnQAndroid.FirebaseObjects;
using Firebase.Xamarin.Database.Query;

namespace OnQAndroid
{
    class CompaniesListViewAdapter : BaseAdapter<string>
    {
        private List<string> mItems;
        private List<bool> mFavs;
        private Context mContext;
        private const string FirebaseURL = "https://onqfirebase.firebaseio.com/";
        bool isFavorite;
        public string companyid;
        public string favoritesFileName;

        public CompaniesListViewAdapter(Context context, List<string> items, List<bool> favs)
        {
            mItems = items;
            mContext = context;
            mFavs = favs;
        }
        public override int Count
        {
            get
            {
                return mItems.Count;           
            }
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override string this[int position]
        {
            get
            {
                return mItems[position];
            }
        }

        View row;
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            row = convertView;

            string dbPath_attributes = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "attributes.db3");
            var db_attributes = new SQLiteConnection(dbPath_attributes);

            var myAttributes = db_attributes.Get<MyAttributes>(1);
            int myCFID = myAttributes.cfid;

            companyid = (position + 1).ToString();

            favoritesFileName = "fav_" + myCFID.ToString() + "_" + myAttributes.loginid.ToString();

            if (row == null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.companieslistview_row, null, false);
            }

            TextView companyName = row.FindViewById<TextView>(Resource.Id.companyName);
            ImageView companyLogo = row.FindViewById<ImageView>(Resource.Id.companyLogo);
            LinearLayout companyInfo = row.FindViewById<LinearLayout>(Resource.Id.companyInfo);
            LinearLayout favorite = row.FindViewById<LinearLayout>(Resource.Id.favorite);
            LinearLayout q_ll = row.FindViewById<LinearLayout>(Resource.Id.q_ll);
            ImageView star = row.FindViewById<ImageView>(Resource.Id.star);

            companyName.Text = mItems[position];
            string fileName = companyName.Text.ToLower().Replace(" ", "");
            int resourceId = (int)typeof(Resource.Drawable).GetField(fileName).GetValue(null);
            companyLogo.SetImageResource(resourceId);

            isFavorite = mFavs[position];

            if (isFavorite == true)
            {
                star.SetImageResource(Resource.Drawable.starfilled);
            }
            else if (isFavorite == false)
            {
                star.SetImageResource(Resource.Drawable.starunfilled);
            }

            q_ll.Click += (sender, e) =>
            {
                Android.Support.V4.App.FragmentTransaction trans = ((FragmentActivity)mContext).SupportFragmentManager.BeginTransaction();
                Fragments.confirmQ fragment = new Fragments.confirmQ();
                Bundle arguments = new Bundle();
                arguments.PutInt("CompanyInt", position + 1);
                fragment.Arguments = arguments;
                trans.Replace(Resource.Id.companies_root_frame, fragment);
                trans.Commit();
            };

            favorite.Click += (sender, e) =>
            {
                if (isFavorite == true)
                {
                    star.SetImageResource(Resource.Drawable.starunfilled);
                    isFavorite = false;
                    mFavs[position] = false;
                    UpdateIsFavorite(isFavorite, position+1);
                }
                else if (isFavorite == false)
                {
                    star.SetImageResource(Resource.Drawable.starfilled);
                    isFavorite = true;
                    mFavs[position] = true;
                    UpdateIsFavorite(isFavorite, position+1);
                }
            };

            companyInfo.Click += (sender, e) =>
            {
                Android.Support.V4.App.FragmentTransaction trans = ((FragmentActivity)mContext).SupportFragmentManager.BeginTransaction();

                CompanyInfoFragment fragment = new CompanyInfoFragment();

                Bundle arguments = new Bundle();

                arguments.PutInt("CompanyInt", position + 1);

                arguments.PutString("Sender", "Companies");
                fragment.Arguments = arguments;
                trans.Replace(Resource.Id.companies_root_frame, fragment);

                trans.Commit();
            };

            return row;
        }

        private async void UpdateIsFavorite(bool newIsFavorite, int companyid)
        {
            var firebase = new FirebaseClient(FirebaseURL);
            var allFavorites = await firebase.Child(favoritesFileName).OnceAsync<Favorite>();
            string key = "";
            string name = "";

            foreach (var favorite in allFavorites)
            {
                if (favorite.Object.companyid == companyid.ToString())
                {
                    key = favorite.Key;
                    name = favorite.Object.name;
                }
            }

            Favorite updateFavorite = new Favorite();
            updateFavorite.companyid = companyid.ToString();
            updateFavorite.isFavorite = newIsFavorite;
            updateFavorite.name = name;

            await firebase.Child(favoritesFileName).Child(key).PutAsync(updateFavorite);
        }
    }
}