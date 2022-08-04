using NAttribute = NabeatsuGenerator.NabeatsuAttribute;

namespace Foo
{
    namespace Bar.Bar.Bar
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
