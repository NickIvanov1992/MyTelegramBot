var lines = File.ReadAllLines("data.txt");
var questions = lines.Select(s => s.Split("|")).Select(s => (s[0], s[1])).ToList();
var random = new Random();
var count = questions.Count;
var score = 0;


while (true)
{
    var index = random.Next(count - 1);
    var question = questions[index];

    var opened = 0;
    while (opened < question.Item2.Length)
    {
        Console.WriteLine($"{question.Item1}: {question.Item2.Length} букв");
        var answer = question.Item2.Substring(0, opened).PadRight(question.Item2.Length, '*');
        Console.WriteLine(answer);
        var tryAnswer = Console.ReadLine().ToLower().Replace('ё','е');
        if (tryAnswer == question.Item2)
        {
            score++;
            Console.WriteLine("Верно!");
            Console.WriteLine($" У вас{score} очков");
            break;
        }
        else
        {
            Console.WriteLine("Не верно!");
            opened++;
        }
    }
     if(opened == question.Item2.Length)
    {
        Console.WriteLine($"Правильный ответ: {question.Item2}");
    }
}

