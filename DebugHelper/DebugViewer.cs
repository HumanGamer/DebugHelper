using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DebugHelper.Util;

namespace DebugHelper
{
    /// <summary>
    /// Displays debug info for an object
    /// </summary>
    public class DebugViewer : Control
    {
        private object _selectedObject;

        /// <summary>
        /// The object for this DebugViewer to display.
        /// </summary>
        public object SelectedObject
        {
            get
            {
                return _selectedObject;
            }
            set
            {
                _selectedObject = value;
                UpdateDisplay();
            }
        }

        private readonly Dictionary<string, object> _fields;
        private readonly ListView _lstView;

        /// <summary>
        /// Initialize a new instance of the DebugViewer class with default settings.
        /// </summary>
        public DebugViewer()
        {
            _fields = new Dictionary<string, object>();
            //DoubleBuffered = true;

            Width = 200;
            Height = 200;

            _lstView = new ListView();
            _lstView.View = View.Details;
            _lstView.Dock = DockStyle.Fill;
            _lstView.FullRowSelect = true;
            _lstView.GridLines = true;
            _lstView.MultiSelect = false;
            _lstView.DoubleClick += LstViewDoubleClick;

            _lstView.Columns.Add("Index", 60);
            _lstView.Columns.Add("Type", 120);
            _lstView.Columns.Add("Name", 120);
            _lstView.Columns.Add("Value", 420);

            _lstView.ColumnWidthChanging += LstViewOnColumnWidthChanging;

            Controls.Add(_lstView);
        }

        private void LstViewOnColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            // Disabled as it's unnessecary
            /*ListView listView = sender as ListView;
            if (listView == null)
                return; // Should never happen
            e.Cancel = true;
            e.NewWidth = listView.Columns[e.ColumnIndex].Width;*/
        }

        private void LstViewDoubleClick(object sender, EventArgs e)
        {
            if (_lstView.SelectedIndices.Count == 0)
                return;
            int index = _lstView.SelectedIndices[0];
            object obj = _fields.Values.ToList()[index];

            ProcessObject(obj);
        }

        private void View_DoubleClick(object sender, EventArgs e)
        {
            SuperListView listView = (SuperListView)sender;
            if (listView.SelectedIndices.Count == 0)
                return;
            int index = listView.SelectedIndices[0];

            if (listView.List == null && listView.Dictionary != null)
            {
                IDictionary dict = listView.Dictionary;
                if (index > dict.Count)
                    return;

                List<object> values = new List<object>();
                foreach (object value in dict.Values)
                {
                    values.Add(value);
                }

                ProcessObject(values[index]);
            }
            else if (listView.List != null && listView.Dictionary == null)
            {
                IList list = listView.List;
                object obj;
                if (list is Array && ((Array) list).Rank > 1)
                {
                    SpecialListViewItem item = listView.SelectedItems[0] as SpecialListViewItem;
                    if (item == null)
                        return;

                    int[] indices = item.SpecialIndex;
                    obj = ((Array) list).GetValue(indices);
                }
                else
                {
                    if (index > list.Count)
                        return;
                    obj = list[index];
                }
                ProcessObject(obj);
            }
        }

        private Type GetBestType1(Type type1, Type type2)
        {
            if (type1 == typeof(object) || type2 == typeof(object))
                return typeof(object);
            Type result;
            if (type1.BaseType == type2)
            {
                result = type2;
            }
            else if (type1 == type2.BaseType)
            {
                result = type1;
            }
            else if (type1.BaseType == type2.BaseType)
            {
                result = type1.BaseType;
            }
            else
            {
                result = GetBestType1(type1.BaseType, type2);
                if (result == null)
                    result = GetBestType1(type1, type2.BaseType);
                if (result == null)
                    result = GetBestType1(type1.BaseType, type2.BaseType);
            }

            return result;
        }

        private List<Type> GetBestType2(List<Type> types)
        {
            List<Type> types2 = new List<Type>();
            bool matches = true;
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                for (int j = 0; j < types.Count; j++)
                {
                    Type type2 = types[j];

                    if (type != type2)
                    {
                        matches = false;
                        Type type3 = GetBestType1(type, type2);
                        if (type3 != null && !types2.Contains(type3))
                            types2.Add(type3);
                    }
                }
            }

            if (matches && types.Count > 0)
            {
                types2.Add(types[0]);
                return types2;
            }

            if (types2.Count > 1)
                return GetBestType2(types2);
            return types2;
        }

        private Type GetArrayType(IList list)
        {
            if (list == null)
                return null;

            List<Type> types = new List<Type>();
            foreach (object obj in list)
            {
                if (obj != null)
                    types.Add(obj.GetType());
            }

            List<Type> types2 = GetBestType2(types);
            if (types2.Count > 0)
                return types2[0];

            return list.GetType();
        }

        private Type GetArrayType(IDictionary dictionary, bool keyType)
        {
            if (dictionary == null)
                return null;

            List<Type> types = new List<Type>();
            ICollection collection = keyType ? dictionary.Keys : dictionary.Values;

            foreach (object obj in collection)
            {
                if (obj != null)
                    types.Add(obj.GetType());
            }

            List<Type> types2 = GetBestType2(types);
            if (types2.Count > 0)
                return types2[0];

            return dictionary.GetType();
        }

        private string GetTypeName(Type type)
        {
            if (type == typeof(ValueType))
                return "Number";
            return type.Name;
        }

        private string GetTypeString(object obj)
        {
            if (obj is IList && !(obj is Array))
            {
                IList listObj = (IList) obj;
                Type type = GetArrayType(listObj);
                if (type != null)
                    return "List<" + GetTypeName(type) + ">";
            } else if (obj is IDictionary)
            {
                IDictionary dictObj = (IDictionary)obj;
                Type keyType = GetArrayType(dictObj, true);
                Type valueType = GetArrayType(dictObj, false);

                if (keyType != null && valueType != null)
                    return "Dictionary<" + GetTypeName(keyType) + ", " + GetTypeName(valueType) + ">";
            }

            if (obj == null)
                return "<null>";
            return GetTypeName(obj.GetType());
        }

        private string GetPrimitiveString(object obj)
        {
            string value;
            if (obj is byte)
            {
                value = "0x" + ((byte)obj).ToString("X2");
            }
            else if (obj is sbyte)
            {
                value = ((sbyte)obj).ToString();
            }
            else if (obj is char)
            {
                value = obj.ToString();
            }
            else if (obj is short)
            {
                value = ((short)obj).ToString();
            }
            else if (obj is ushort)
            {
                value = "0x" + ((ushort)obj).ToString("X4");
            }
            else if (obj is int)
            {
                value = ((int)obj).ToString();
            }
            else if (obj is uint)
            {
                value = "0x" + ((uint)obj).ToString("X8");
            }
            else if (obj is long)
            {
                value = ((long)obj).ToString();
            }
            else if (obj is ulong)
            {
                value = "0x" + ((ulong)obj).ToString("X16");
            }
            else if (obj is float)
            {
                value = ((float)obj).ToString("0.00");
            }
            else if (obj is double)
            {
                value = ((double)obj).ToString("0.0000");
            }
            else
            {
                value = obj.ToString();
            }
            return value;
        }

        private List<ListViewItem> GetMultiItems(Array values, int[] indices, int dimensionNum)
        {
            // http://csharphelper.com/blog/2016/12/loop-over-an-array-of-unknown-dimension-in-c/
            // by Rod Stephens

            List<ListViewItem> result = new List<ListViewItem>();
            int maxIndex = values.GetUpperBound(dimensionNum);
            for (int i = 0; i <= maxIndex; i++)
            {
                indices[dimensionNum] = i;

                if (dimensionNum == values.Rank - 2)
                {
                    result.AddRange(GetMultiInner(values, indices));
                }
                else
                {
                    result.AddRange(GetMultiItems(values, indices, dimensionNum + 1));
                }
            }

            return result;
        }

        private List<ListViewItem> GetMultiInner(Array values, int[] indices)
        {
            // http://csharphelper.com/blog/2016/12/loop-over-an-array-of-unknown-dimension-in-c/
            // by Rod Stephens

            List<ListViewItem> result = new List<ListViewItem>();

            int dimensionNum = values.Rank - 1;
            int maxIndex = values.GetUpperBound(dimensionNum);
            for (int i = 0; i <= maxIndex; i++)
            {
                indices[dimensionNum] = i;

                object o = values.GetValue(indices);

                string value = "<null>";
                if (o != null)
                    value = o.ToString();
                if (o != null && o.GetType().IsPrimitive || o is string)
                    value = GetPrimitiveString(o);

                //string type = "<null>";
                //if (o != null)
                string type = GetTypeString(o);
                string[] item =
                {
                    "[" + indices.AsArrayIndexString() + "]",
                    type,
                    value
                };
                result.Add(new SpecialListViewItem(indices, item));
            }

            return result;
        }

        private void ProcessObject(object obj)
        {
            if (obj == null)
                return;
            if (obj.GetType().IsPrimitive || obj is string)
            {
                string value = GetPrimitiveString(obj);

                Form editForm = new Form
                {
                    FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    Size = new Size(287 + 16, 44 + 39), // 16 is the x border size and 39 is the y border size
                    StartPosition = FormStartPosition.CenterParent,
                    ShowInTaskbar = false,
                    Text = "View Value"
                };

                TextBox txtBox = new TextBox
                {
                    Location = new Point(12, 12),
                    Size = new Size(176, 20),
                    ReadOnly = true,
                    Text = value
                };

                SuperButton btnOk = new SuperButton(editForm, txtBox)
                {
                    Location = new Point(176 + 24, 11),
                    Size = new Size(75, 22),
                    Text = "OK"
                };
                btnOk.Click += BtnOk_Click;

                editForm.Controls.Add(txtBox);
                editForm.Controls.Add(btnOk);

                editForm.AcceptButton = btnOk;

                editForm.ShowDialog(this);
                
            }
            else if (obj is IList)
            {
                var array = obj as Array;
                if (array != null && array.Rank > 1)
                {
                    Array arr = array;

                    Form listForm = new Form
                    {
                        Text = "View Array",
                        Size = new Size(640, 480),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.SizableToolWindow,
                        MaximizeBox = false,
                        MinimizeBox = false,
                        ShowInTaskbar = false
                    };

                    SuperListView view = new SuperListView(arr)
                    {
                        View = View.Details,
                        Dock = DockStyle.Fill,
                        FullRowSelect = true,
                        GridLines = true,
                        MultiSelect = false,
                    };

                    view.DoubleClick += View_DoubleClick;

                    view.Columns.Add("Index", 60);
                    view.Columns.Add("Type", 120);
                    view.Columns.Add("Value", 540);

                    view.ColumnWidthChanging += LstViewOnColumnWidthChanging;

                    int[] indices = new int[arr.Rank];
                    List<ListViewItem> items = GetMultiItems(arr, indices, 0);
                    view.Items.AddRange(items.ToArray());

                    listForm.Controls.Add(view);

                    listForm.ShowDialog(this);
                }
                else
                {
                    IList listObj = (IList) obj;

                    string txt = "View List";
                    if (listObj is Array)
                        txt = "View Array";

                    Form listForm = new Form
                    {
                        Text = txt,
                        Size = new Size(640, 480),
                        StartPosition = FormStartPosition.CenterParent,
                        FormBorderStyle = FormBorderStyle.SizableToolWindow,
                        MaximizeBox = false,
                        MinimizeBox = false,
                        ShowInTaskbar = false
                    };

                    SuperListView view = new SuperListView(listObj)
                    {
                        View = View.Details,
                        Dock = DockStyle.Fill,
                        FullRowSelect = true,
                        GridLines = true,
                        MultiSelect = false
                    };

                    view.DoubleClick += View_DoubleClick;

                    view.Columns.Add("Index", 60);
                    view.Columns.Add("Type", 120);
                    view.Columns.Add("Value", 540);

                    view.ColumnWidthChanging += LstViewOnColumnWidthChanging;

                    for (int i = 0; i < listObj.Count; i++)
                    {
                        string value = "<null>";
                        if (listObj[i] != null)
                            value = listObj[i].ToString();
                        if (listObj[i] != null && listObj[i].GetType().IsPrimitive || listObj[i] is string)
                            value = GetPrimitiveString(listObj[i]);
                        string type = "<null>";
                        if (listObj[i] != null)
                            type = GetTypeString(listObj[i]);
                        string[] listItem =
                        {
                            i.ToString(),
                            type,
                            value
                        };
                        view.Items.Add(new ListViewItem(listItem));
                    }

                    listForm.Controls.Add(view);

                    listForm.ShowDialog(this);
                }
            }
            else if (obj is IDictionary)
            {
                IDictionary dictObj = (IDictionary)obj;

                Form listForm = new Form
                {
                    Text = "View Dictionary",
                    Size = new Size(640, 480),
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.SizableToolWindow,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ShowInTaskbar = false
                };

                SuperListView view = new SuperListView(dictObj)
                {
                    View = View.Details,
                    Dock = DockStyle.Fill,
                    FullRowSelect = true,
                    GridLines = true,
                    MultiSelect = false
                };

                view.DoubleClick += View_DoubleClick;

                view.Columns.Add("Key", 60);
                view.Columns.Add("Type", 120);
                view.Columns.Add("Value", 540);

                view.ColumnWidthChanging += LstViewOnColumnWidthChanging;

                ICollection keyCollection = dictObj.Keys;
                ICollection valueCollection = dictObj.Values;

                List<object> keys = new List<object>();
                foreach (object key in keyCollection)
                {
                    keys.Add(key);
                }

                List<object> values = new List<object>();
                foreach (object value in valueCollection)
                {
                    values.Add(value);
                }

                for (int i = 0; i < dictObj.Count; i++)
                {
                    string key = keys[i].ToString();
                    string value = "<null>";
                    if (values[i] != null)
                        value = values[i].ToString();
                    if (values[i] != null && values[i].GetType().IsPrimitive || values[i] is string)
                        value = GetPrimitiveString(values[i]);
                    string type = "<null>";
                    if (values[i] != null)
                        type = GetTypeString(values[i]);
                    string[] listItem =
                    {
                        key,
                        type,
                        value
                    };
                    view.Items.Add(new ListViewItem(listItem));
                }

                listForm.Controls.Add(view);

                listForm.ShowDialog(this);
            }
            else
            {
                Form dlg = BuildDebugDlg(obj);
                dlg.ShowDialog(this);
            }
        }

        public static Form BuildDebugDlg(object obj)
        {
            Form form = new Form
            {
                Text = "View Object",
                Size = new Size(640, 480),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.SizableToolWindow,
                MaximizeBox = false,
                MinimizeBox = false,
                ShowInTaskbar = false
            };

            DebugViewer view = new DebugViewer
            {
                Dock = DockStyle.Fill,
                SelectedObject = obj
            };

            form.Controls.Add(view);

            return form;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            SuperButton button = (SuperButton)sender;

            button.Form.Close();
        }

        private void UpdateDisplay()
        {
            _fields.Clear();
            _lstView.Items.Clear();
            object obj = _selectedObject;
            if (obj == null)
                return;
            Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields();
            foreach (FieldInfo field in fields)
            {
                _fields.Add(field.Name, field.GetValue(obj));
            }

            PropertyInfo[] properties = t.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                try
                {
                    _fields.Add(property.Name, property.GetValue(obj));
                }
                catch (TargetParameterCountException e)
                {
                    Debug.WriteLine(e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
            }

            List<string> keys = _fields.Keys.ToList();
            List<object> values = _fields.Values.ToList();
            for (int i = 0; i < _fields.Count; i++)
            {
                string value;
                if (values[i] == null)
                    value = "<null>";
                else if (values[i].GetType().IsPrimitive || values[i] is string)
                    value = GetPrimitiveString(values[i]);
                else if (values[i] is IList)
                {
                    if (values[i] is Array && ((Array) values[i]).Rank > 1)
                    {
                        Array arr = (Array) values[i];
                        int[] indices = new int[arr.Rank];

                        for (int k = 0; k < arr.Rank; k++)
                        {
                            indices[k] = arr.GetLength(k);
                        }

                        value = "Array[" + indices.AsArrayIndexString() + "]";
                    }
                    else
                    {
                        IList list = (IList) values[i];
                        if (list is Array)
                            value = "Array[" + list.Count + "]";
                        else
                            value = "List[" + list.Count + "]";
                    }
                }
                else if (values[i] is IDictionary)
                {
                    IDictionary dict = (IDictionary) values[i];
                    value = "Dictionary[" + dict.Count + "]";
                }
                else
                    value = values[i].ToString();
                string type = "<null>";
                if (values[i] != null)
                    type = GetTypeString(values[i]);
                string[] listItem = 
                {
                    i.ToString(),
                    type,
                    keys[i],
                    value
                };
                _lstView.Items.Add(new ListViewItem(listItem));
            }
        }

        private class SuperListView : ListView
        {
            public IList List
            {
                get;
                private set;
            }

            public IDictionary Dictionary
            {
                get;
                private set;
            }

            public SuperListView(IList list) : base()
            {
                List = list;
                Dictionary = null;
            }

            public SuperListView(IDictionary dictionary) : base()
            {
                List = null;
                Dictionary = dictionary;
            }
        }

        private class SuperButton : Button
        {
            public Form Form
            {
                get;
                private set;
            }

            public TextBox TextBox
            {
                get;
                private set;
            }

            public SuperButton(Form form, TextBox textBox) : base()
            {
                Form = form;
                TextBox = textBox;
            }
        }

        private class SpecialListViewItem : ListViewItem
        {
            public int[] SpecialIndex
            {
                get;
                private set;
            }

            public SpecialListViewItem(int[] specialIndex, string[] subItems) : base(subItems)
            {
                SpecialIndex = specialIndex;
            }
        }
    }
}
