using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using Luval.UiPath.Recorder.Activities.Design.Designers;
using Luval.UiPath.Recorder.Activities.Design.Properties;

namespace Luval.UiPath.Recorder.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(StartLuvalRecording), categoryAttribute);
            builder.AddCustomAttributes(typeof(StartLuvalRecording), new DesignerAttribute(typeof(StartLuvalRecordingDesigner)));
            builder.AddCustomAttributes(typeof(StartLuvalRecording), new HelpKeywordAttribute(""));

            builder.AddCustomAttributes(typeof(StopLuvalRecording), categoryAttribute);
            builder.AddCustomAttributes(typeof(StopLuvalRecording), new DesignerAttribute(typeof(StopLuvalRecordingDesigner)));
            builder.AddCustomAttributes(typeof(StopLuvalRecording), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
