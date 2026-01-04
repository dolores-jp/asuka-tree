using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AsukaTree
{
    public static class JsonItemsParser
    {
        public static JsonTreeNode[] ParseItemsOnly(string json)
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("items", out var itemsEl) ||
                itemsEl.ValueKind != JsonValueKind.Array)
            {
                return Array.Empty<JsonTreeNode>();
            }

            var list = new List<JsonTreeNode>();
            foreach (var item in itemsEl.EnumerateArray())
            {
                // items配列の要素が最上段（itemsノードは表示しない）
                list.Add(BuildItemNode(item, allowChildren: true));
            }
            return list.ToArray();
        }

        private static JsonTreeNode BuildItemNode(JsonElement item, bool allowChildren)
        {
            int type = TryGetInt(item, "type") ?? -1;
            string name = TryGetString(item, "name") ?? "(no-name)";
            int? num0 = TryGetNum0(item);
            string? seals = TryGetSealConcat(item);
            int potCount = (type == 10) ? TryGetPotCount(item) : 0;

            string text = BuildDisplayText(name, type, num0, seals, potCount);

            var node = new JsonTreeNode
            {
                Text = text,
                IconKey = $"type:{type}"
            };

            // type=10（壺）だけ、pot配列を子として1段表示
            if (allowChildren && type == 10)
            {
                if (item.TryGetProperty("pot", out var potEl) && potEl.ValueKind == JsonValueKind.Array)
                {
                    foreach (var inner in potEl.EnumerateArray())
                    {
                        node.Children.Add(BuildItemNode(inner, allowChildren: false)); // 子は展開しない
                    }
                }
            }

            return node;
        }

        private static string BuildDisplayText(string name, int type, int? num0, string? seals, int potCount)
        {
            // 剣盾
            if (type == 0 || type == 1)
            {
                var sb = new StringBuilder();
                sb.Append(name);

                if (num0.HasValue)
                {
                    if (num0.Value > 0) sb.Append($"+{num0.Value}");
                    else if (num0.Value < 0) sb.Append(num0.Value.ToString());
                }

                if (!string.IsNullOrEmpty(seals))
                {
                    sb.Append(' ');
                    sb.Append('[');
                    sb.Append(seals);
                    sb.Append(']');
                }

                return sb.ToString();
            }

            // 矢
            if (type == 3)
            {
                if (num0.HasValue) return $"{name} [{num0.Value}]";
                return name;
            }

            // 石
            if (type == 4)
            {
                if (num0.HasValue) return $"{name} [{num0.Value}]";
                return name;
            }

            // 巻物
            if (type == 7)
            {
                // 白紙の巻物(○○の巻物) → 白紙:○○
                var m = Regex.Match(
                    name,
                    @"^白紙の巻物\((.+?)の巻物\)$"
                );

                if (m.Success)
                {
                    name = $"白紙:{m.Groups[1].Value}";
                }

                return name;
            }

            // 杖
            if (type == 9)
            {
                //未使用識別だと-65536
                if (num0.HasValue) return (num0.Value < -100 ? $"{name}" : $"{name} [{num0.Value}]");
                return name;
            }

            // 壺
            if (type == 10)
            {
                if (num0.HasValue)
                {
                    int remain = num0.Value - potCount; // 容量 - 中身数
                    return $"{name} [{remain}]";
                }
                return name;
            }

            // エレキ箱
            //if (type == 11)
            //{
            //    if (num0.HasValue) return $"{name} [{num0.Value}]";
            //    return name;
            //}

            // ギタン
            if (type == 12)
            {
                if (num0.HasValue) return $"{num0.Value}{name}";
                return name;
            }

            return name;
        }

        private static int? TryGetNum0(JsonElement item)
        {
            if (!item.TryGetProperty("num", out var numEl) || numEl.ValueKind != JsonValueKind.Array)
                return null;

            using var e = numEl.EnumerateArray();
            if (!e.MoveNext()) return null;

            var first = e.Current;
            if (first.ValueKind == JsonValueKind.Number && first.TryGetInt32(out var n)) return n;

            return null;
        }

        private static string? TryGetSealConcat(JsonElement item)
        {
            if (!item.TryGetProperty("seal", out var sealEl) || sealEl.ValueKind != JsonValueKind.Array)
                return null;

            var sb = new StringBuilder();
            foreach (var s in sealEl.EnumerateArray())
            {
                sb.Append(s.ToString()); // 区切り無し連結
            }
            return sb.Length == 0 ? null : sb.ToString();
        }

        private static string? TryGetString(JsonElement obj, string propName)
        {
            if (obj.ValueKind != JsonValueKind.Object) return null;
            if (!obj.TryGetProperty(propName, out var p)) return null;
            if (p.ValueKind == JsonValueKind.String) return p.GetString();
            return p.ToString();
        }

        private static int? TryGetInt(JsonElement obj, string propName)
        {
            if (obj.ValueKind != JsonValueKind.Object) return null;
            if (!obj.TryGetProperty(propName, out var p)) return null;
            if (p.ValueKind == JsonValueKind.Number && p.TryGetInt32(out var n)) return n;
            return null;
        }
        
        private static int TryGetPotCount(JsonElement item)
        {
            if (!item.TryGetProperty("pot", out var potEl) || potEl.ValueKind != JsonValueKind.Array)
                return 0;

            int count = 0;
            foreach (var _ in potEl.EnumerateArray())
                count++;

            return count;
        }
    }
}
