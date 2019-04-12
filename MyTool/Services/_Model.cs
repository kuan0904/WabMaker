using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web.ModelBinding;

namespace MyTool.Services
{
    public class _Model
    {
        public static string GetDisplayName<T>(string column)
        {
            string name = column;

            //display name
            MemberInfo property = typeof(T).GetProperty(column);
            var attribute = property.GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().FirstOrDefault();

            if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
            {
                name = attribute.Name;
            }
            else
            {
                //display name in MetaData
                var metadataType = typeof(T).GetCustomAttributes(typeof(MetadataTypeAttribute), true).OfType<MetadataTypeAttribute>().FirstOrDefault();
                var metaData = (metadataType != null)
                    ? ModelMetadataProviders.Current.GetMetadataForType(null, metadataType.MetadataClassType)
                    : ModelMetadataProviders.Current.GetMetadataForType(null, typeof(T));

                var propertMetaData = metaData.Properties.FirstOrDefault(x => x.PropertyName == column);
                if (propertMetaData != null && !string.IsNullOrEmpty(propertMetaData.DisplayName)) {
                    name = propertMetaData.DisplayName;
                }             
            }
            
            return name;
        }

        /// <summary>
        /// CheckBoxList 轉為有check 的GuidList
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        public static List<Guid> ToCheckedGuid(List<CheckBoxListItem> model)
        {
            return model?.Where(x => x.IsChecked).Select(x => x.ID).ToList();            
        }


        public static Type GetType(string name)
        {
            var targetType = Type.GetType(name);
            if (targetType != null)
            {
                return targetType;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name == name)
                    {
                        return type;
                    }
                }
            }

            return null;
        }
    }
}
