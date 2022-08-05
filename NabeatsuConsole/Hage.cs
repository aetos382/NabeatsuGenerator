using NAttribute = NabeatsuGenerator.NabeatsuAttribute;

namespace Foo
{
    namespace Bar1.Bar2.Bar3
    {
        namespace Baz
        {
            public partial class Hoge
            {
                public partial class Hige
                {
                    public partial class Hage
                    {
                        [N(2110000200, 2110000204)]
                        public static partial IEnumerable<string> GenerateNabeatsuSequence();
                    }
                }
            }
        }
    }
}
