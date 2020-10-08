using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HRC.OzoneContacts.DL;

namespace HRC.OzoneContacts.BL
{
    public class OzonePostResultValue
    {
        public string Dict { get; set; }
        public string Status { get; set; }
        public string Value { get; set; }
    }

    public class OzonePostResultItem
    {
        public string Id { get; set; }
        public string Filename { get; set; }
        public string Status { get; set; }
        public List<OzonePostResultValue> Values { get; set; }

        private int ValueSuccessCount
        {
            get
            {
                return this.Values.FindAll(delegate(OzonePostResultValue v)
                {
                    return v.Status.Equals("SUCCESS");
                }).Count;
            }
        }

        internal bool AllValuesSuccessful
        {
            get
            {
                return this.ValueSuccessCount == this.Values.Count;
            }
        }

        public OzonePostResultItem()
        {
            this.Values = new List<OzonePostResultValue>();
        }
    }

    public class OzonePostResult
    {
        public List<OzonePostResultItem> Items { get; set; }
        public List<string> Errors { get; set; }

        private int ItemSuccessCount
        {
            get
            {
                return this.Items.FindAll(delegate(OzonePostResultItem r)
                {
                    return r.Status.Equals("SUCCESS") && r.AllValuesSuccessful;
                }).Count;
            }
        }

        private bool AllItemsSuccessful
        {
            get
            {
                return this.ItemSuccessCount == this.Items.Count;
            }
        }

        private bool AnyItemsSuccessful
        {
            get
            {
                return this.ItemSuccessCount > 0;
            }
        }

        public OzoneQueueStatus Status
        {
            get
            {
                if (this.Errors.Count == 0 && this.AllItemsSuccessful)
                {
                    return OzoneQueueStatus.FullCompletion;                    
                }
                else if (this.AnyItemsSuccessful)
                {
                    return OzoneQueueStatus.PartialCompletion;
                }
                else
                {
                    return OzoneQueueStatus.Failed;
                }
            }
        }

        public OzonePostResult()
        {
            this.Items = new List<OzonePostResultItem>();
            this.Errors = new List<string>();
        }

        public void Save(string xml, int queueId)
        {
            OzoneContactManager.SavePostResult(this, xml, queueId);
        }
    }
}
