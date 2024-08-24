using SmartFormat;
using SmartFormat.Core.Settings;
using SmartFormat.Extensions;


namespace WINX.Utils
{
    
    // Uses <c>string.Format</c>-compatible escaping of curly braces, {{ and }},
    /// instead of the <c>Smart.Format</c> default escaping, \{ and \}.
    /// <para>Custom formatters cannot be parsed / used, if set to <see langword="true"/><<<---IMPORTANT
    // defaultSmartFormat.Settings.StringFormatCompatibility = false; //should be false and will use \{ instead of {{
    public class DictionarySmartFormatter
    {
        private static SmartFormatter __formatter;

        public static SmartFormatter Formatter
        {
            get
            {
                if (__formatter == null)
                {
                    __formatter = CreateDefaultSmartFormat();
                }

                return __formatter;
            }
        }

        private static SmartFormatter CreateDefaultSmartFormat(SmartSettings? settings = null)
        {
            // Register all default extensions here:
            var smart = new SmartFormatter(settings)
                // Extension are sorted automatically
                .AddExtensions(
                    new StringSource(),
                    // will automatically be added to the IFormatter list, too
                    new ListFormatter(),
                    new CustomDictionarySource(),
                    // new DictionarySource(),
                    // new ValueTupleSource(),
                    // new ReflectionSource(),
                    // for string.Format behavior
                    new DefaultSource()
                    // new KeyValuePairSource()
                )
                .AddExtensions(
                    new DictionaryObjectKeyFormatter(),
                    new PluralLocalizationFormatter(),
                    new ConditionalFormatter(),
                    new IsMatchFormatter(),
                    new NullFormatter(),
                    new ChooseFormatter(),
                    new SubStringFormatter(),
                    // for string.Format behavior
                    new DefaultFormatter()
                );


            // smart.Settings.StringFormatCompatibility = true;// {{<==duplication for escape, no custom formaters,
            // also doesn't work with deep level of nesting {0:{current_action:{properties:{description}}}}
            //default
            //smart.Settings.StringFormatCompatibility = false;// \{<== should be escaped
            
            return smart;
        }
    }
}