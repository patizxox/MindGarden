using System.Collections.Generic;

namespace MindGarden
{
    public record Quote(int Id, string Text, string Author);

    public static class QuotesData
    {
        public static readonly List<Quote> All = new()
        {
            new Quote(1, "Nie to, co nam się przydarza, ale to, jak na to reagujemy, decyduje o naszym szczęściu.", "Epiktet"),
            new Quote(2, "Szczęście zależy od nas samych.", "Arystoteles"),
            new Quote(3, "Bądź zmianą, którą pragniesz ujrzeć w świecie.", "Mahatma Gandhi"),
            new Quote(4, "Cierpliwość jest gorzka, ale jej owoce są słodkie.", "Jean-Jacques Rousseau"),
            new Quote(5, "Nawet najdłuższa podróż zaczyna się od pierwszego kroku.", "Laozi"),
            new Quote(6, "Jedynym sposobem na wykonanie świetnej pracy jest kochanie tego, co robisz.", "Steve Jobs"),
            new Quote(7, "Chwytaj dzień, bo żaden się nie powtórzy.", "Horacy"),
            new Quote(8, "Spokój jest kluczem do panowania nad sobą.", "Platon"),
            new Quote(9, "To, co robisz dzisiaj, może poprawić całe twoje jutro.", "Ralph Marston"),
            new Quote(10, "Życie to nie czekanie, aż burza minie. To nauka, jak tańczyć w deszczu.", "Vivian Greene"),
            new Quote(11, "Nie ma drogi do szczęścia. To szczęście jest drogą.", "Thich Nhat Hanh"),
            new Quote(12, "Zatrzymaj się i powąchaj różę.", "Przysłowie"),
        };
    }
}
