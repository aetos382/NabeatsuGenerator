# NabeatsuGenerator
3 の倍数と 3 がつく数の時だけアホになる Source Generator です。

# 経緯
https://twitter.com/Sheeeeepla/status/1554415675212693504

# 使い方
適当なメソッドに `NabeatsuAttribute` をつけます。対象のメソッドは `partial` で、かつ、戻り値は `IEnumerable<string>` である必要があります。
属性のパラメーターで最大値と最小値を指定します。

```cs
[Nabeatsu(1, 100)]
public partial IEnumerable<string> GetNabeatsuSequence();
```

すると、以下のようなコードが生成されます。

```cs
public partial IEnumerable<string> GetNabeatsuSequence()
{
    yield return "1";
    yield return "2";
    yield return "さん";
    yield return "4";
    yield return "5";
    yield return "ろく";
    yield return "7";
    yield return "8";
    yield return "きゅう";
    yield return "10";
    // 以下省略
}
```
