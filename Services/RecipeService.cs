using ForkFeedMobile.Models;

namespace ForkFeedMobile.Services;

public class RecipeService
{
    private readonly List<Recipe> _recipes;

    public RecipeService()
    {
        _recipes = GenerateMockRecipes();
    }

    public async Task<List<Recipe>> GetRecipesAsync(int page = 0, int pageSize = 6,
        string? search = null, string? difficulty = null, string? sortBy = null)
    {
        await Task.Delay(600);

        IEnumerable<Recipe> query = _recipes;

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Title.Contains(search, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(difficulty) && difficulty != "All")
            query = query.Where(r => r.Difficulty.Equals(difficulty, StringComparison.OrdinalIgnoreCase));

        query = sortBy switch
        {
            "Date" => query.OrderByDescending(r => r.CreatedAt),
            "Difficulty" => query.OrderBy(r => r.Difficulty switch
            {
                "Easy" => 0,
                "Medium" => 1,
                "Hard" => 2,
                _ => 3
            }),
            "Rating" => query.OrderByDescending(r => r.Rating),
            _ => query.OrderByDescending(r => r.CreatedAt)
        };

        return query.Skip(page * pageSize).Take(pageSize).ToList();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        await Task.Delay(300);
        return _recipes.FirstOrDefault(r => r.Id == id);
    }
    public async Task AddRecipeAsync(Recipe recipe)
    {
        await Task.Delay(400);
        recipe.Id = _recipes.Max(r => r.Id) + 1;
        recipe.CreatedAt = DateTime.Now;
        _recipes.Insert(0, recipe);
    }

    private static List<Recipe> GenerateMockRecipes()
    {
        return new List<Recipe>
        {
            new()
            {
                Id = 1,
                Title = "Classic Margherita Pizza",
                Description = "A simple yet delicious Italian classic with fresh mozzarella and basil.",
                ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=600",
                Difficulty = "Easy",
                TimeMinutes = 30,
                Rating = 4.7,
                CreatedAt = DateTime.Now.AddDays(-2),
                Ingredients = new()
                {
                    new() { Name = "Pizza dough", Quantity = "1 ball" },
                    new() { Name = "Tomato sauce", Quantity = "1/2 cup" },
                    new() { Name = "Fresh mozzarella", Quantity = "200g" },
                    new() { Name = "Fresh basil", Quantity = "handful" },
                    new() { Name = "Olive oil", Quantity = "1 tbsp" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Preheat oven to 250°C (480°F)." },
                    new() { StepNumber = 2, Description = "Roll out dough into a thin circle on a floured surface." },
                    new() { StepNumber = 3, Description = "Spread tomato sauce evenly over the dough." },
                    new() { StepNumber = 4, Description = "Tear mozzarella and distribute over the sauce." },
                    new() { StepNumber = 5, Description = "Bake for 8-10 minutes until crust is golden." },
                    new() { StepNumber = 6, Description = "Top with fresh basil and a drizzle of olive oil. Serve hot." }
                }
            },
            new()
            {
                Id = 2,
                Title = "Chicken Tikka Masala",
                Description = "Creamy, spiced tomato curry with tender chicken pieces.",
                ImageUrl = "https://images.unsplash.com/photo-1565557623262-b51c2513a641?w=600",
                Difficulty = "Medium",
                TimeMinutes = 50,
                Rating = 4.9,
                CreatedAt = DateTime.Now.AddDays(-5),
                Ingredients = new()
                {
                    new() { Name = "Chicken breast", Quantity = "500g" },
                    new() { Name = "Yogurt", Quantity = "1 cup" },
                    new() { Name = "Garam masala", Quantity = "2 tsp" },
                    new() { Name = "Tomato puree", Quantity = "400ml" },
                    new() { Name = "Heavy cream", Quantity = "1/2 cup" },
                    new() { Name = "Onion", Quantity = "1 large" },
                    new() { Name = "Garlic", Quantity = "4 cloves" },
                    new() { Name = "Ginger", Quantity = "1 inch" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Marinate chicken in yogurt and spices for 30 minutes." },
                    new() { StepNumber = 2, Description = "Grill or pan-fry chicken until charred." },
                    new() { StepNumber = 3, Description = "Sauté onion, garlic, and ginger until soft." },
                    new() { StepNumber = 4, Description = "Add tomato puree and simmer 15 minutes." },
                    new() { StepNumber = 5, Description = "Stir in cream and cooked chicken. Simmer 10 minutes." },
                    new() { StepNumber = 6, Description = "Serve with basmati rice or naan bread." }
                }
            },
            new()
            {
                Id = 3,
                Title = "Beef Wellington",
                Description = "An impressive dish with tender beef fillet wrapped in puff pastry.",
                ImageUrl = "https://images.unsplash.com/photo-1600891964092-4316c288032e?w=600",
                Difficulty = "Hard",
                TimeMinutes = 120,
                Rating = 4.5,
                CreatedAt = DateTime.Now.AddDays(-1),
                Ingredients = new()
                {
                    new() { Name = "Beef fillet", Quantity = "1 kg" },
                    new() { Name = "Puff pastry", Quantity = "1 sheet" },
                    new() { Name = "Mushrooms", Quantity = "400g" },
                    new() { Name = "Prosciutto", Quantity = "8 slices" },
                    new() { Name = "Dijon mustard", Quantity = "2 tbsp" },
                    new() { Name = "Egg yolk", Quantity = "1" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Sear beef on all sides in a hot pan. Brush with mustard." },
                    new() { StepNumber = 2, Description = "Blitz mushrooms into a paste (duxelles) and cook until dry." },
                    new() { StepNumber = 3, Description = "Lay prosciutto on cling film, spread duxelles, place beef and roll tightly." },
                    new() { StepNumber = 4, Description = "Chill for 30 minutes." },
                    new() { StepNumber = 5, Description = "Wrap in puff pastry, egg-wash, and score the top." },
                    new() { StepNumber = 6, Description = "Bake at 200°C for 25-30 minutes until pastry is golden." }
                }
            },
            new()
            {
                Id = 4,
                Title = "Fresh Caesar Salad",
                Description = "Crispy romaine lettuce with homemade Caesar dressing and croutons.",
                ImageUrl = "https://images.unsplash.com/photo-1546793665-c74683f339c1?w=600",
                Difficulty = "Easy",
                TimeMinutes = 15,
                Rating = 4.2,
                CreatedAt = DateTime.Now.AddDays(-10),
                Ingredients = new()
                {
                    new() { Name = "Romaine lettuce", Quantity = "2 heads" },
                    new() { Name = "Parmesan cheese", Quantity = "50g" },
                    new() { Name = "Croutons", Quantity = "1 cup" },
                    new() { Name = "Anchovy fillets", Quantity = "4" },
                    new() { Name = "Egg yolk", Quantity = "1" },
                    new() { Name = "Lemon juice", Quantity = "2 tbsp" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Wash and chop romaine lettuce." },
                    new() { StepNumber = 2, Description = "Blend anchovy, egg yolk, lemon juice, and oil for dressing." },
                    new() { StepNumber = 3, Description = "Toss lettuce with dressing." },
                    new() { StepNumber = 4, Description = "Top with croutons and shaved parmesan." }
                }
            },
            new()
            {
                Id = 5,
                Title = "Japanese Ramen",
                Description = "Rich tonkotsu broth with noodles, soft egg, and chashu pork.",
                ImageUrl = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=600",
                Difficulty = "Hard",
                TimeMinutes = 180,
                Rating = 4.8,
                CreatedAt = DateTime.Now.AddDays(-3),
                Ingredients = new()
                {
                    new() { Name = "Pork bones", Quantity = "1 kg" },
                    new() { Name = "Ramen noodles", Quantity = "400g" },
                    new() { Name = "Soft-boiled eggs", Quantity = "4" },
                    new() { Name = "Pork belly", Quantity = "300g" },
                    new() { Name = "Soy sauce", Quantity = "4 tbsp" },
                    new() { Name = "Green onions", Quantity = "4 stalks" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Boil pork bones for 8-12 hours to create tonkotsu broth." },
                    new() { StepNumber = 2, Description = "Braise pork belly in soy sauce and mirin for 2 hours." },
                    new() { StepNumber = 3, Description = "Marinate soft-boiled eggs in soy mixture for 4 hours." },
                    new() { StepNumber = 4, Description = "Cook ramen noodles according to package." },
                    new() { StepNumber = 5, Description = "Assemble: noodles, broth, sliced pork, egg, and green onions." }
                }
            },
            new()
            {
                Id = 6,
                Title = "Chocolate Lava Cake",
                Description = "Warm chocolate cake with a gooey molten center.",
                ImageUrl = "https://images.unsplash.com/photo-1624353365286-3f8d62daad51?w=600",
                Difficulty = "Medium",
                TimeMinutes = 25,
                Rating = 4.6,
                CreatedAt = DateTime.Now.AddDays(-7),
                Ingredients = new()
                {
                    new() { Name = "Dark chocolate", Quantity = "200g" },
                    new() { Name = "Butter", Quantity = "100g" },
                    new() { Name = "Eggs", Quantity = "3" },
                    new() { Name = "Sugar", Quantity = "80g" },
                    new() { Name = "Flour", Quantity = "30g" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Melt chocolate and butter together." },
                    new() { StepNumber = 2, Description = "Whisk eggs and sugar until pale and fluffy." },
                    new() { StepNumber = 3, Description = "Fold chocolate mixture into eggs, then fold in flour." },
                    new() { StepNumber = 4, Description = "Pour into greased ramekins." },
                    new() { StepNumber = 5, Description = "Bake at 200°C for exactly 12 minutes." },
                    new() { StepNumber = 6, Description = "Invert onto plates and serve immediately." }
                }
            },
            new()
            {
                Id = 7,
                Title = "Greek Moussaka",
                Description = "Layered eggplant casserole with spiced meat and béchamel.",
                ImageUrl = "https://images.unsplash.com/photo-1586190848861-99aa4a171e90?w=600",
                Difficulty = "Medium",
                TimeMinutes = 90,
                Rating = 4.4,
                CreatedAt = DateTime.Now.AddDays(-4),
                Ingredients = new()
                {
                    new() { Name = "Eggplants", Quantity = "3 large" },
                    new() { Name = "Ground lamb", Quantity = "500g" },
                    new() { Name = "Tomato sauce", Quantity = "400ml" },
                    new() { Name = "Milk", Quantity = "500ml" },
                    new() { Name = "Flour", Quantity = "3 tbsp" },
                    new() { Name = "Butter", Quantity = "3 tbsp" },
                    new() { Name = "Nutmeg", Quantity = "1/4 tsp" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Slice and salt eggplants. Let drain 30 minutes, then fry." },
                    new() { StepNumber = 2, Description = "Brown lamb with onion, add tomato sauce and spices. Simmer." },
                    new() { StepNumber = 3, Description = "Make béchamel: melt butter, stir in flour, add milk gradually." },
                    new() { StepNumber = 4, Description = "Layer eggplant, meat sauce, eggplant, then béchamel on top." },
                    new() { StepNumber = 5, Description = "Bake at 180°C for 40 minutes until golden." }
                }
            },
            new()
            {
                Id = 8,
                Title = "Avocado Toast",
                Description = "Simple, trendy, and packed with healthy fats.",
                ImageUrl = "https://images.unsplash.com/photo-1541519227354-08fa5d50c44d?w=600",
                Difficulty = "Easy",
                TimeMinutes = 10,
                Rating = 4.0,
                CreatedAt = DateTime.Now.AddDays(-12),
                Ingredients = new()
                {
                    new() { Name = "Sourdough bread", Quantity = "2 slices" },
                    new() { Name = "Ripe avocado", Quantity = "1" },
                    new() { Name = "Lemon juice", Quantity = "1 tsp" },
                    new() { Name = "Chili flakes", Quantity = "pinch" },
                    new() { Name = "Salt & pepper", Quantity = "to taste" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Toast sourdough slices until golden." },
                    new() { StepNumber = 2, Description = "Mash avocado with lemon juice, salt, and pepper." },
                    new() { StepNumber = 3, Description = "Spread on toast and sprinkle with chili flakes." }
                }
            },
            new()
            {
                Id = 9,
                Title = "Pad Thai",
                Description = "Sweet, sour, and savory Thai stir-fried noodles.",
                ImageUrl = "https://images.unsplash.com/photo-1559314809-0d155014e29e?w=600",
                Difficulty = "Medium",
                TimeMinutes = 35,
                Rating = 4.7,
                CreatedAt = DateTime.Now.AddDays(-6),
                Ingredients = new()
                {
                    new() { Name = "Rice noodles", Quantity = "250g" },
                    new() { Name = "Shrimp", Quantity = "200g" },
                    new() { Name = "Eggs", Quantity = "2" },
                    new() { Name = "Bean sprouts", Quantity = "1 cup" },
                    new() { Name = "Tamarind paste", Quantity = "3 tbsp" },
                    new() { Name = "Fish sauce", Quantity = "2 tbsp" },
                    new() { Name = "Peanuts", Quantity = "1/4 cup" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Soak rice noodles in warm water for 20 minutes." },
                    new() { StepNumber = 2, Description = "Make sauce: mix tamarind, fish sauce, sugar, and lime." },
                    new() { StepNumber = 3, Description = "Stir-fry shrimp until pink, push aside, scramble eggs." },
                    new() { StepNumber = 4, Description = "Add drained noodles and sauce. Toss until coated." },
                    new() { StepNumber = 5, Description = "Add bean sprouts, top with peanuts and lime wedge." }
                }
            },
            new()
            {
                Id = 10,
                Title = "Tiramisu",
                Description = "Classic Italian coffee-flavoured layered dessert.",
                ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=600",
                Difficulty = "Medium",
                TimeMinutes = 40,
                Rating = 4.8,
                CreatedAt = DateTime.Now.AddDays(-8),
                Ingredients = new()
                {
                    new() { Name = "Mascarpone cheese", Quantity = "500g" },
                    new() { Name = "Ladyfinger biscuits", Quantity = "24" },
                    new() { Name = "Espresso", Quantity = "300ml" },
                    new() { Name = "Eggs", Quantity = "4" },
                    new() { Name = "Sugar", Quantity = "100g" },
                    new() { Name = "Cocoa powder", Quantity = "for dusting" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Separate eggs. Beat yolks with sugar until pale." },
                    new() { StepNumber = 2, Description = "Fold mascarpone into yolk mixture." },
                    new() { StepNumber = 3, Description = "Whip egg whites to stiff peaks and fold in gently." },
                    new() { StepNumber = 4, Description = "Dip ladyfingers briefly in espresso and layer in dish." },
                    new() { StepNumber = 5, Description = "Spread mascarpone cream, repeat layers." },
                    new() { StepNumber = 6, Description = "Refrigerate for at least 4 hours. Dust with cocoa before serving." }
                }
            },
            new()
            {
                Id = 11,
                Title = "Fish Tacos",
                Description = "Crispy battered fish with tangy slaw in warm tortillas.",
                ImageUrl = "https://images.unsplash.com/photo-1551504734-5ee1c4a1479b?w=600",
                Difficulty = "Easy",
                TimeMinutes = 25,
                Rating = 4.3,
                CreatedAt = DateTime.Now.AddDays(-9),
                Ingredients = new()
                {
                    new() { Name = "White fish fillets", Quantity = "400g" },
                    new() { Name = "Corn tortillas", Quantity = "8" },
                    new() { Name = "Cabbage", Quantity = "2 cups shredded" },
                    new() { Name = "Lime", Quantity = "2" },
                    new() { Name = "Sour cream", Quantity = "1/2 cup" },
                    new() { Name = "Flour", Quantity = "1/2 cup" },
                    new() { Name = "Beer", Quantity = "1/2 cup" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Make batter with flour, beer, and a pinch of salt." },
                    new() { StepNumber = 2, Description = "Dip fish in batter and fry until golden and crispy." },
                    new() { StepNumber = 3, Description = "Toss cabbage with lime juice and a pinch of salt." },
                    new() { StepNumber = 4, Description = "Warm tortillas. Fill with fish, slaw, and sour cream." }
                }
            },
            new()
            {
                Id = 12,
                Title = "Mushroom Risotto",
                Description = "Creamy Arborio rice with mixed wild mushrooms and parmesan.",
                ImageUrl = "https://images.unsplash.com/photo-1476124369491-e7addf5db371?w=600",
                Difficulty = "Medium",
                TimeMinutes = 45,
                Rating = 4.5,
                CreatedAt = DateTime.Now.AddDays(-11),
                Ingredients = new()
                {
                    new() { Name = "Arborio rice", Quantity = "300g" },
                    new() { Name = "Mixed mushrooms", Quantity = "300g" },
                    new() { Name = "Vegetable stock", Quantity = "1 litre" },
                    new() { Name = "White wine", Quantity = "150ml" },
                    new() { Name = "Parmesan", Quantity = "80g" },
                    new() { Name = "Butter", Quantity = "30g" },
                    new() { Name = "Shallot", Quantity = "1" }
                },
                Steps = new()
                {
                    new() { StepNumber = 1, Description = "Sauté sliced mushrooms in butter until golden. Set aside." },
                    new() { StepNumber = 2, Description = "Cook shallot in the same pan, add rice, toast 2 minutes." },
                    new() { StepNumber = 3, Description = "Deglaze with white wine, stir until absorbed." },
                    new() { StepNumber = 4, Description = "Add stock one ladle at a time, stirring until absorbed." },
                    new() { StepNumber = 5, Description = "Stir in mushrooms, parmesan, and a knob of butter." }
                }
            }
        };
    }
}
