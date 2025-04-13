using AutomationOfPurchases.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AutomationOfPurchases.API.Infrastructure.Seed
{
    public static class DatabaseSeeder
    {
        public static async Task SeedTestDataAsync(
            AppDbContext db,
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager)
        {
            // 1) Застосовуємо (за потреби) міграції
            db.Database.Migrate();

            // 2) Створюємо (якщо треба) ролі
            var roles = new[] { "User", "DepartmentHead", "Economist", "WarehouseWorker" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 3) Створюємо 5 департаментів, якщо ще не існують
            if (!db.Departments.Any())
            {
                var finance = new Department { DepartmentName = "Департамент фінансів", FreeCapital = 40000 };
                var sales = new Department { DepartmentName = "Департамент продажів", FreeCapital = 20000 };
                var itDept = new Department { DepartmentName = "Департамент IT", FreeCapital = 25000 };
                var whDept = new Department { DepartmentName = "Департамент першого складу", FreeCapital = 8000 };
                var mach = new Department { DepartmentName = "Машинобудівний департамент", FreeCapital = 30000 };

                db.Departments.AddRange(finance, sales, itDept, whDept, mach);
                await db.SaveChangesAsync();

                Console.WriteLine("Created 5 departments: Finance, Sales, IT, Warehouse, Machinery");
            }

            // Зчитаємо департаменти з БД
            var finDept = db.Departments.First(d => d.DepartmentName == "Департамент фінансів");
            var salesDept = db.Departments.First(d => d.DepartmentName == "Департамент продажів");
            var itDeptDb = db.Departments.First(d => d.DepartmentName == "Департамент IT");
            var whDeptDb = db.Departments.First(d => d.DepartmentName == "Департамент першого складу");
            var machDept = db.Departments.First(d => d.DepartmentName == "Машинобудівний департамент");

            // 4) Створюємо економістів, ЯКІ ФІЗИЧНО ПЕРЕБУВАЮТЬ У ФІНАНСОВОМУ ВІДДІЛІ
            //    (тобто departmentId = finDept, щоб їх заявки затверджував керівник фіндепу).
            var ecoOneId = await CreateUserIfNotExists(
                userManager,
                userName: "ecoOne",
                password: "EcoOne123!",
                fullName: "Єфименко Євген Вікторович",
                roleName: "Economist",
                departmentId: finDept.DepartmentId
            );
            var ecoTwoId = await CreateUserIfNotExists(
                userManager,
                userName: "ecoTwo",
                password: "EcoTwo123!",
                fullName: "Лучко Людмила Петрівна",
                roleName: "Economist",
                departmentId: finDept.DepartmentId
            );

            // 5) Прив’язуємо цих економістів до додаткових відділів (через DepartmentEconomists)
            //    ecoOne -> (fin, sales, it)
            //    ecoTwo -> (fin, warehouse, mach)
            if (!string.IsNullOrEmpty(ecoOneId))
            {
                await CreateEconomistInDepartments(db, ecoOneId, finDept.DepartmentId, salesDept.DepartmentId, itDeptDb.DepartmentId);
            }
            if (!string.IsNullOrEmpty(ecoTwoId))
            {
                await CreateEconomistInDepartments(db, ecoTwoId, whDeptDb.DepartmentId, machDept.DepartmentId);
            }

            // 6) Керівник фіндепу (єдиний у цьому відділі)
            var finHeadId = await CreateUserIfNotExists(
                userManager,
                userName: "finHead",
                password: "FinHead123!",
                fullName: "Ковальчук Олександр Миколайович",
                roleName: "DepartmentHead",
                departmentId: finDept.DepartmentId
            );
            if (!string.IsNullOrEmpty(finHeadId))
            {
                finDept.HeadOfDepartmentId = finHeadId;
                db.Departments.Update(finDept);
                await db.SaveChangesAsync();
            }

            // 7) Департамент продажів: 2 user, 1 head
            await CreateUserIfNotExists(
                userManager,
                userName: "salesUser1",
                password: "SalesUser1!",
                fullName: "Романенко Роман Юрійович",
                roleName: "User",
                departmentId: salesDept.DepartmentId
            );
            await CreateUserIfNotExists(
                userManager,
                userName: "salesUser2",
                password: "SalesUser2!",
                fullName: "Мельник Микола Васильович",
                roleName: "User",
                departmentId: salesDept.DepartmentId
            );
            var salesHeadId = await CreateUserIfNotExists(
                userManager,
                userName: "salesHead",
                password: "SalesHead123!",
                fullName: "Гончаренко Андрій Петрович",
                roleName: "DepartmentHead",
                departmentId: salesDept.DepartmentId
            );
            if (!string.IsNullOrEmpty(salesHeadId))
            {
                salesDept.HeadOfDepartmentId = salesHeadId;
                db.Departments.Update(salesDept);
                await db.SaveChangesAsync();
            }

            // 8) Департамент IT: 2 user, 1 head
            await CreateUserIfNotExists(
                userManager,
                userName: "itUser1",
                password: "ItUser1!",
                fullName: "Ярошенко Ярослав Богданович",
                roleName: "User",
                departmentId: itDeptDb.DepartmentId
            );
            await CreateUserIfNotExists(
                userManager,
                userName: "itUser2",
                password: "ItUser2!",
                fullName: "Бондаренко Богдан Михайлович",
                roleName: "User",
                departmentId: itDeptDb.DepartmentId
            );
            var itHeadId = await CreateUserIfNotExists(
                userManager,
                userName: "itHead",
                password: "ItHead123!",
                fullName: "Волошин Ігор Вікторович",
                roleName: "DepartmentHead",
                departmentId: itDeptDb.DepartmentId
            );
            if (!string.IsNullOrEmpty(itHeadId))
            {
                itDeptDb.HeadOfDepartmentId = itHeadId;
                db.Departments.Update(itDeptDb);
                await db.SaveChangesAsync();
            }

            // 9) Департамент першого складу: 2 user (WarehouseWorker), 1 head (WarehouseWorker + DeptHead)
            await CreateUserIfNotExists(
                userManager,
                userName: "whUser1",
                password: "WhUser1!",
                fullName: "Лисенко Леонід Васильович",
                roleName: "WarehouseWorker",
                departmentId: whDeptDb.DepartmentId
            );
            await CreateUserIfNotExists(
                userManager,
                userName: "whUser2",
                password: "WhUser2!",
                fullName: "Остапенко Олег Анатолійович",
                roleName: "WarehouseWorker",
                departmentId: whDeptDb.DepartmentId
            );
            var whHeadId = await CreateUserIfNotExists(
                userManager,
                userName: "whHead",
                password: "WhHead123!",
                fullName: "Савченко Сергій Олександрович",
                roleName: "WarehouseWorker",
                departmentId: whDeptDb.DepartmentId
            );
            if (!string.IsNullOrEmpty(whHeadId))
            {
                // Додаємо йому додатково роль DepartmentHead
                var whHeadUser = await userManager.FindByIdAsync(whHeadId);
                await userManager.AddToRoleAsync(whHeadUser, "DepartmentHead");

                // Прописуємо в сам департамент
                whDeptDb.HeadOfDepartmentId = whHeadId;
                db.Departments.Update(whDeptDb);
                await db.SaveChangesAsync();
            }

            // 10) Машинобудівний департамент: 2 user + 1 head
            await CreateUserIfNotExists(
                userManager,
                userName: "machUser1",
                password: "MachUser1!",
                fullName: "Степаненко Степан Степанович",
                roleName: "User",
                departmentId: machDept.DepartmentId
            );
            await CreateUserIfNotExists(
                userManager,
                userName: "machUser2",
                password: "MachUser2!",
                fullName: "Опря Олексій Романович",
                roleName: "User",
                departmentId: machDept.DepartmentId
            );
            var machHeadId = await CreateUserIfNotExists(
                userManager,
                userName: "machHead",
                password: "MachHead123!",
                fullName: "Дорошенко Дмитро Іванович",
                roleName: "DepartmentHead",
                departmentId: machDept.DepartmentId
            );
            if (!string.IsNullOrEmpty(machHeadId))
            {
                machDept.HeadOfDepartmentId = machHeadId;
                db.Departments.Update(machDept);
                await db.SaveChangesAsync();
            }

            // 11) Додаємо товарні позиції (Items), якщо треба
            await SeedItemsFromListAsync(db);

            Console.WriteLine("Database seeding completed with 5 departments, 2 economists in finance, no regular users in finance!");
        }

        /// <summary>
        /// Прив’язує користувача-економіста (userId) до переліку відділів (departmentIds),
        /// додаючи запис DepartmentEconomist для кожного.
        /// </summary>
        private static async Task CreateEconomistInDepartments(AppDbContext db, string economistUserId, params int[] departmentIds)
        {
            // Користувач уже належить до якоїсь DeptId, але тут робимо many-to-many зв’язки
            foreach (var deptId in departmentIds)
            {
                var exists = db.DepartmentEconomists.Any(de =>
                    de.DepartmentId == deptId && de.EconomistId == economistUserId);
                if (!exists)
                {
                    var link = new DepartmentEconomist
                    {
                        DepartmentId = deptId,
                        EconomistId = economistUserId
                    };
                    db.DepartmentEconomists.Add(link);
                    Console.WriteLine($"Economist '{economistUserId}' прив’язаний до DepartmentId={deptId}");
                }
            }
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Створює користувача з указаними параметрами, якщо його ще не існує;
        /// повертає Id створеного/існуючого користувача.
        /// </summary>
        private static async Task<string?> CreateUserIfNotExists(
            UserManager<AppUser> userManager,
            string userName,
            string password,
            string fullName,
            string roleName,
            int? departmentId)
        {
            var existingUser = await userManager.FindByNameAsync(userName);
            if (existingUser == null)
            {
                var user = new AppUser
                {
                    UserName = userName,
                    Email = userName + "@test.com",
                    FullName = fullName,
                    DepartmentId = departmentId
                };

                var createResult = await userManager.CreateAsync(user, password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, roleName);
                    Console.WriteLine($"User '{userName}' created, role '{roleName}', deptId={departmentId}.");
                    return user.Id;
                }
                else
                {
                    var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                    Console.WriteLine($"Failed to create user '{userName}': {errors}");
                    return null;
                }
            }
            else
            {
                Console.WriteLine($"User '{userName}' already exists. Skipping.");
                return existingUser.Id;
            }
        }

        /// <summary>
        /// Якщо треба додати кілька Item, якщо ще не існують у БД.
        /// </summary>
        private static async Task SeedItemsFromListAsync(AppDbContext db)
        {
            if (!db.Items.Any())
            {
                var items = new List<Item>
                {
                  new Item { ItemName = "ЛИСТ ЗІ СТАЛІ 500HBW б=40мм", StorageUnit = "т" },
new Item { ItemName = "МЕТАЛОКОНСТРУКЦІЇ 1725.2-1-106.11-КМ", StorageUnit = "т" },
new Item { ItemName = "МЕТАЛОКОНСТРУКЦІЇ 1725.2-1-106.11-КМ", StorageUnit = "т" },
new Item { ItemName = "РЕШІТКОВИЙ НАСТИЛ ПРЕСОВАНИЙ KOP 22*22/30*3 оц", StorageUnit = "т" },
new Item { ItemName = "ФУТЕРУВАЛЬНИЙ МАТЕРІАЛ \"КОРУНДОВА ПЛИТКА AL2O3 НА ГНУЧКІЙ ОСНОВІ 494*965мм", StorageUnit = "м2" },
new Item { ItemName = "ФУТЕРУВАЛЬНИЙ МАТЕРІАЛ \"КОРУНДОВА ПЛИТКА AL2O3 НА ГНУЧКІЙ ОСНОВІ 494*965мм", StorageUnit = "м2" },
new Item { ItemName = "БОРТ ГУМОВИЙ (БЕЗ КОРДА) АМ.62-583.00.02-01", StorageUnit = "шт" },
new Item { ItemName = "ОБИЧАЙКА КОРПУСУ ПЕЧІ 40.4442.001", StorageUnit = "шт" },
new Item { ItemName = "ЕЛЕКТРОДИ ГРАФІТОВАНІ В КОМПЛЕКТІ З НІПЕЛЯМИ Ф 200 ММ", StorageUnit = "т" },
new Item { ItemName = "МУФТА D=50мм", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ф32 ЭК", StorageUnit = "шт" },
new Item { ItemName = "МУФТА В-В Мод.0270 09 \"GENEBRE\"", StorageUnit = "шт" },
new Item { ItemName = "МУФТА СТАЛЕВА ПРЯМА Ду50 ГОСТ 8966-75", StorageUnit = "шт" },
new Item { ItemName = "ТРІЙНИК РІВНОПРОХОДНИЙ D=325*10мм ГОСТ 17376-2001", StorageUnit = "шт" },
new Item { ItemName = "ПАТРУБОК ГУМОВО-МЕТАЛЕВИЙ З ФЛАНЦЕМ D=300; L=515", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ПЕРЕХІДНА МІДНА D= 42*22", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ПЕРЕХІДНА СТАЛЬНА d= 42*15", StorageUnit = "шт" },
new Item { ItemName = "ПЕРЕХІД ГУМОВИЙ D400 X300", StorageUnit = "шт" },
new Item { ItemName = "ПЕРЕХІД СТАЛЕВИЙ 57*5-25*3 ГОСТ 17378-2001", StorageUnit = "шт" },
new Item { ItemName = "ПЕРЕХІД СТАЛЕВИЙ К 325*8-159*6", StorageUnit = "шт" },
new Item { ItemName = "ПЕРЕХІД СТАЛЕВИЙ 720*14-630*14 ОСТ 3622-77", StorageUnit = "шт" },
new Item { ItemName = "ПЕРЕХІД СТАЛЕВИЙ К 2-325*8-219*7 ГОСТ 17378;2003", StorageUnit = "шт" },
new Item { ItemName = "ПЕРЕХІД СТАЛЕВИЙ К 426*12-219*10мм ДСТУ ГОСТ 17378:2003", StorageUnit = "шт" },
new Item { ItemName = "ПЕРЕХІД СТАЛЕВИЙ К 42,4*3,6-33,7*3,2 ст.20 ДСТУ ГОСТ 17378:2003", StorageUnit = "шт" },
new Item { ItemName = "ЗАГЛУШКА 325*10 ДСТУ ГОСТ 17 379:2003", StorageUnit = "шт" },
new Item { ItemName = "ЗАГЛУШКА D=283 (АМ.62-28.00.36)", StorageUnit = "шт" },
new Item { ItemName = "ЗАГЛУШКА СТАЛЕВА 76*3 ГОСТ 17379-2003", StorageUnit = "шт" },
new Item { ItemName = "ЗАГЛУШКА ФЛАНЦЕВА 1-300-10 ГОСТ 12836-67", StorageUnit = "шт" },
new Item { ItemName = "ФУТОРКА Ду 32*15", StorageUnit = "шт" },
new Item { ItemName = "ЗГІН G 1/2-А (94.4987.001)", StorageUnit = "шт" },
new Item { ItemName = "ЗГІН-АМЕРИКАНКА ПРЯМИЙ 1 1/2\"", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=325*8мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=325*8мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=325*8мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=219*8мм ДСТУ ГОСТ 17375-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=219*8мм ДСТУ ГОСТ 17375-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=133*6мм ДСТУ ГОСТ 17375-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=133*6мм ДСТУ ГОСТ 17375-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=133*6мм ДСТУ ГОСТ 17375-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД ЗАГНУТИЙ 90град. D=219*4мм ДСТУ ГОСТ 17375-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=40*3,5мм ДСТУ ГОСТ 17375-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=426*10мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=426*10мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 60град. D=325*8мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 45град. D=57*5мм ДСТУ ГОСТ 17378-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=108*3,5мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 45град. D=325*16мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 45град. D=325*16мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 45град. D=530*16мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 60град. D=159*14мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=273*8мм ДСТУ ГОСТ 17375-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 60град. D=325*10мм", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 45град. D=720*10мм", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 45град. D=219*8мм", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=219*14мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 45град. D=325*14мм ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=426*14мм ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 30град. D=325*14мм ДСТУ ГОСТ 17375-2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 45град. D=159*14мм ДСТУ ГОСТ 17375:2003", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД МЕТАЛЕВИЙ ОБГУМОВАНИЙ 15-1-355,6*1 (2.3735-253-ТХ2)", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД МЕТАЛЕВИЙ ОБГУМОВАНИЙ 17-1-355,6*1 (2.3735-253-ТХ2)", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД МЕТАЛЕВИЙ ОБГУМОВАНИЙ 31-1-355,6*1 (2.3735-253-ТХ2)", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. D=426*12мм", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 60град. D=273*14мм ДСТУ ГОСТ 17375", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 90град. Ду50", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД СТАЛЕВИЙ 30град. D=102*6мм 17375-2001", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД ГУМОВИЙ 90град. D=325мм 3430/6-101.3-ТК 1.1", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД ГУМОВАНИЙ (КОМПЛЕКТНО: З ФЛАНЦЕМ, ВІДПОВІДНИМ ФЛАНЦЕМ ПІД ТРУБУ D=426 і КРІПЛЕННЯ) Ду400 37град.", StorageUnit = "шт" },
new Item { ItemName = "ВІДВІД ГУМОВИЙ D=426 ОТР-Н 426.95.02 ВО", StorageUnit = "шт" },
new Item { ItemName = "КОМПЛЕКТ ЗВОРОТНИХ ФЛАНЦІВ З ПРОКЛАДКАМИ ТА КРІПЛЕННЯМ DN200 PN16", StorageUnit = "компл" },
new Item { ItemName = "ФЛАНЕЦЬ D=630, d=520 (АМ.62-28.00.02)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=565, d=430 (АМ.62-28.00.05)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=345, d=230 (АМ.62-28.00.06)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=450, d=325 (АМ.62-28.00.07)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=538, d=325 (АМ.62-28.00.13)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=505, d=345 (АМ.62-28.00.14)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=640, d=530 (АМ.62-28.00.18)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=600, d=430 (АМ.62-28.00.20)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=535, d=370 (АМ.62-28.00.22)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=820, d=690 (АМ.62-28.00.24)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=193, d=91 (АМ.62-28.00.32)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=234, d=134 (АМ.62-28.00.33)", StorageUnit = "шт" },
new Item { ItemName = "ФЛАНЕЦЬ D=640, d=320 (АМ.62-28.00.35)", StorageUnit = "шт" },
new Item { ItemName = "ВСТАВКА ГУМОВА  Ду150 56.2778.004", StorageUnit = "шт" },
new Item { ItemName = "ПЕРЕТВОРЮВАЧ ЧАСТОТИ З ПАНЕЛЛЮ УПРАВЛІННЯ ACS550-01-087A-4", StorageUnit = "шт" },
new Item { ItemName = "SEW ПЕРЕТВОРЮВАЧ ЧАСТОТНИЙ 206401915", StorageUnit = "шт" },
new Item { ItemName = "СТРОП 4СК1 12,5/4500", StorageUnit = "шт" },
new Item { ItemName = "СТРОП 4СК 10,0/3000 ГОСТ 25573-82", StorageUnit = "шт" },
new Item { ItemName = "БУФЕР ГОРИЗОНТАЛЬНИЙ 11431.2.4СБ", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПРОМІЖНИЙ 11431.10СБ", StorageUnit = "шт" },
new Item { ItemName = "ТЯГЛО 561331073", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ ГАЛЬМІВНИЙ D=400мм С282.423", StorageUnit = "шт" },
new Item { ItemName = "СИСТЕМА ПЕРЕСУВАННЯ РУКАВА Дв25 мм FESTOON", StorageUnit = "шт" },
new Item { ItemName = "БОЛТ М64*300 4-86778", StorageUnit = "шт" },
new Item { ItemName = "ШПОНКА 40*22*180 (110.4739.001)", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ 1510*1190*160", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА 3020, D=55мм, SA952801", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА (ДЛЯ ЕЛЕКТРОДВИГУНА \"БАРМАК\") B96AT02", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА (869.0283-00) MGT Nr125 Di95mm", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА для 61-564130 \"МЕТСО\" кат.61-586287", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА MGT Nr125 Di110 (869.0286-00)", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА MGT Nr125 Di110 (869.0286-00)", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА MGT Nr125 Di110 (869.0286-00)", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ (ДЛЯ ЕЛЕКТРОДВИГУНА \"БАРМАК\") B96AT15", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ PHP 4SPC200TB SKF", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ SPC 335 MGT 125-10 (879.0511-00)", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ SPC 400 MGT 125-12 (879.0499-00)", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ ВЕДЕНИЙ SPC 5.265", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ ВЕДЕНИЙ SPC 5.265-01", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ SPC 500 MGT 125 (879.0497-00)", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ SPC 500 MGT 125 (879.0497-00)", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ SPC 500 MGT 125 (879.0497-00)", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ З ВТУЛКОЮ В ЗБОРІ SPC265-03 (TB 3535)+TB 3535 d=48mm", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ У ЗБОРІ З ВТУЛКОЮ PHP 3SPC200TB (SKF)", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-4", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-6", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-6", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-6", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-7", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-7", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-8", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-9", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-9", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ЗУБЧАСТА МЗ-9", StorageUnit = "шт" },
new Item { ItemName = "НАПІВМУФТА З ЧОРНОВИМ ОТВОРОМ РНЕ FRC90RSB", StorageUnit = "шт" },
new Item { ItemName = "ТРУБА ГУМОВО-МЕТАЛЕВА D=200; L=665", StorageUnit = "шт" },
new Item { ItemName = "ТРУБА ГУМОВА D=426; L=910", StorageUnit = "шт" },
new Item { ItemName = "ТРУБА ГУМОВО-МЕТАЛЕВА D=300; L=1050", StorageUnit = "шт" },
new Item { ItemName = "ТРУБА ГУМОВО-МЕТАЛЕВА D=250; L=1010", StorageUnit = "шт" },
new Item { ItemName = "ВОРОТА РАСПАШНЫЕ АМ.01-622.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "ВОРОТА РАСПАШНЫЕ АМ.01-631.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "ВОРОНКА ЗАВАНТАЖЕННЯ НАСОСУ ТА-42А НПВ 110-2 639-105-КМ (арк.1-3)", StorageUnit = "шт" },
new Item { ItemName = "ЩИТ ПЕРЕКРИТТЯ Щ1 2658.1-1-КБ02", StorageUnit = "шт" },
new Item { ItemName = "ЩИТ ПЕРЕКРИТТЯ Щ2 2658.1-1-КБ02", StorageUnit = "шт" },
new Item { ItemName = "ЩИТ ПЕРЕКРИТТЯ Щ3 2658.1-1-КБ02", StorageUnit = "шт" },
new Item { ItemName = "ЩИТ ПЕРЕКРИТТЯ Щ4 2658.1-1-КБ02.1", StorageUnit = "шт" },
new Item { ItemName = "ПЛАСТИНА 6*130*1500", StorageUnit = "шт" },
new Item { ItemName = "ПЛАСТИНА ПРАВА  10*320*2170(62.13336.04) (ЗАМОВЛЕННЯ 0201568)", StorageUnit = "шт" },
new Item { ItemName = "ПЛАСТИНА ПРАВА  10*320*2170(62.13336.04) (ЗАМОВЛЕННЯ 0201568)", StorageUnit = "шт" },
new Item { ItemName = "ПЛАСТИНА ПРАВА  10*320*2170(62.13336.04) (ЗАМОВЛЕННЯ 0201568)", StorageUnit = "шт" },
new Item { ItemName = "ОПОРА МЕТАЛЕВА ОМ1 13105.Р90/273.1-1-21-КБ", StorageUnit = "шт" },
new Item { ItemName = "ЗАГОТОВКА ВКЛАДИША 43.3828.002 (ЗАКАЗ 0402419)", StorageUnit = "шт" },
new Item { ItemName = "ЗАГОТІВЛЯ ВТУЛКИ НАТЯЖНОГО КОЛЕСА 3502.05.02.302 БрА9Ж3Л", StorageUnit = "шт" },
new Item { ItemName = "ПОКОВКА МАТОЧИНИ ГС.62-599.00.01 (ЗАКАЗ 0302350)", StorageUnit = "шт" },
new Item { ItemName = "ПОКОВКА МАТОЧИНИ ГС.62-569.00.01 (ЗАКАЗ 0302340)", StorageUnit = "шт" },
new Item { ItemName = "ПОКОВКА ВТУЛКИ ТРАВЕРЗИ М608-11/2 ГОСТ 380-11 (ЗАМОВЛЕННЯ 0406560)", StorageUnit = "шт" },
new Item { ItemName = "ПОКОВКА КОРПУСУ 43.3828.001 А (ЗАМОВЛЕННЯ 0406901)", StorageUnit = "шт" },
new Item { ItemName = "ЗАГОТІВКА КОРПУСА 31122.625.231А.ЗАГ СТ. 3 ГОСТ 380-94 (З/З 0406318 И 0406319)", StorageUnit = "шт" },
new Item { ItemName = "ПОКОВКА ШЕСТІРНІ D=320*d=80*223 (ЗАМОВЛЕННЯ 0402626)", StorageUnit = "шт" },
new Item { ItemName = "ГІДРОЗАМОК ЗМ10 091-60.280151-03", StorageUnit = "шт" },
new Item { ItemName = "ПОКОВКА ПІВМУФТИ 430*100", StorageUnit = "шт" },
new Item { ItemName = "КОЖУХ ЗАВИТКА R=3000 62-12253.04.06.000СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ (кр.БП.800.1.000СБ) З.11164 КОД 531010", StorageUnit = "шт" },
new Item { ItemName = "62.11677.00.00.00.СБ БАРАБАН ПОВІДНИЙ D495", StorageUnit = "шт" },
new Item { ItemName = "КОНВЕЄР (БЕЗ СТРІЧКИ) 2658.1-6-ТХ2.1 КС-6-3", StorageUnit = "шт" },
new Item { ItemName = "КОНВЕЄР (БЕЗ СТРІЧКИ) 2658.1-7-ТХ2.1 КС-6-4", StorageUnit = "шт" },
new Item { ItemName = "КОНВЕЄР (БЕЗ СТРІЧКИ) 2658.1-6-ТХ2.2 КС-7-3", StorageUnit = "шт" },
new Item { ItemName = "КОНВЕЄР (БЕЗ СТРІЧКИ) 2658.1-7-ТХ2.2 КС-7-4", StorageUnit = "шт" },
new Item { ItemName = "62.12368.00.00.00.СБ БАРАБАН ВІДХИЛЮВАЛЬНИЙ D325", StorageUnit = "шт" },
new Item { ItemName = "62.12033.00.00.00.СБ БАРАБАН ВІДХИЛЮВАЛЬНИЙ D800", StorageUnit = "шт" },
new Item { ItemName = "62.12098.00.00.00.СБ БАРАБАН ВІДХИЛЮВАЛЬНИЙ D420", StorageUnit = "шт" },
new Item { ItemName = "62.11686.00.00.00.СБ БАРАБАН ВІДХИЛЮВАЛЬНИЙ D630", StorageUnit = "шт" },
new Item { ItemName = "62.11686.00.00.00.СБ БАРАБАН ВІДХИЛЮВАЛЬНИЙ D630", StorageUnit = "шт" },
new Item { ItemName = "РОЛИК СПЕЦІАЛЬНИЙ", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПОВІДНИЙ 41185-01.000СБ (ЗАМОВЛЕННЯ 0401419)", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 160.80.120.000Б СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 160.63.100.000В СБ", StorageUnit = "шт" },
new Item { ItemName = "ШНЕК 83.4134.000СБ", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ ГАЛЬМІВНИЙ 2805155-1", StorageUnit = "шт" },
new Item { ItemName = "МУФТА СПЕЦІАЛЬНА ДОПОМІЖНОГО ПРИВОДУ 62.12889.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН  ПОВІДНИЙ БН1200.100СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ГОЛОВНИЙ 26.4165.000СБ", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  63.4153.001", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ БП1250.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 100.320.80.000СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН КІНЦЕВИЙ 1400-1250-160.000АСБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН D630*1400 89-669", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 14.2632.000СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 14.2632.000СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 14.2632.000СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 14.2739.100СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН D=630*1150 (89-708)", StorageUnit = "шт" },
new Item { ItemName = "РАМА ПРИВІДНОГО БАРАБАНА 62-13227.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ  D=630 (62.13074.00.00.00СБ)", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ  D=630 (62.13074.00.00.00СБ)", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ВІДХИЛЮВАЛЬНИЙ 38.509.00.000.СБ", StorageUnit = "шт" },
new Item { ItemName = "ЗУПИННИК ХРАПОВИЙ 63.3221.000СБ (ЗАМОВЛЕННЯ 0402084)", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН D=500 ОФ-И-291АСБ (ЗАМОВЛЕННЯ 0301425)", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  84.3581.001", StorageUnit = "шт" },
new Item { ItemName = "ГІДРОЦИЛІНДР ХОД 204мм,Qmax=2000фут/дм2", StorageUnit = "шт" },
new Item { ItemName = "ГІДРОЦИЛІНДР ХОД 204мм,Qmax=2000фут/дм2", StorageUnit = "шт" },
new Item { ItemName = "ГІДРОЦИЛІНДР ХОД 204мм,Qmax=2000фут/дм2", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ГОЛОВНИЙ 31128.200.310СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ГОЛОВНИЙ 31128.200.310СБ", StorageUnit = "шт" },
new Item { ItemName = "РАМА НЕРОБОЧОГО БАРАБАНА 62-13464.02.00СБ", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА 31128.01.017 (ЗАМОВЛЕННЯ 20199/0402691)", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  26.4447.001 (ЗАМОВЛЕННЯ 30472/0403328)", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН D=630 ОФ-И-314", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН D=630 ОФ-И-314", StorageUnit = "шт" },
new Item { ItemName = "НАСОС ГІДРАВЛІЧНИЙ З  РЕГУЛЯТОРОМ ТИСКУ ДМ-6011-Y", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ВІДХИЛЮВАЛЬНИЙ (ЦПТ) D=1250*1800", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ  (ЦПТ) D=1600*1800", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ  (ЦПТ) D=1600*1800", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  25.1844.001 (ЗАМОВЛЕННЯ 20275/0400963)", StorageUnit = "шт" },
new Item { ItemName = "РАМА ХРАПОВОГО ЗУПИННИКА 2Б05470.1-0", StorageUnit = "шт" },
new Item { ItemName = "РАМА ХРАПОВОГО ЗУПИННИКА 2Б05470.1-0", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 100.63-100", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 100.63-100", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 100.63-100", StorageUnit = "шт" },
new Item { ItemName = "ТРУБОПРОВІД ГНУЧКИЙ 25.4747.000АСБ", StorageUnit = "шт" },
new Item { ItemName = "ТРУБОПРОВІД ГНУЧКИЙ 25.4747.000АСБ", StorageUnit = "шт" },
new Item { ItemName = "ОЧИСНИК КОНВЕЄРНОЇ СТРІЧКИ В=1000", StorageUnit = "шт" },
new Item { ItemName = "РОЛИК З ПОДОВЖЕНИМ ВАЛОМ 152*1800", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  54.3319.001", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН D=800*1400", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН D=800*1400", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  54.4250.001 (ЗАМОВЛЕННЯ 20756/0404867)", StorageUnit = "шт" },
new Item { ItemName = "ОЧИСНИК КОНВЕЄРНОЇ СТРІЧКИ ГС.62-305.00.00АСБ (ЗАМОВЛЕННЯ 0201693)", StorageUnit = "шт" },
new Item { ItemName = "ОЧИСНИК КОНВЕЄРНОЇ СТРІЧКИ ГС.62-305.00.00АСБ (ЗАМОВЛЕННЯ 0201693)", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ D=2000*2200ТЗ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ D=2000*2200ТЗ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ D=2000*1800ТЗ", StorageUnit = "шт" },
new Item { ItemName = "ЗМІННЕ ПОЛІУРЕТАНОВЕ ЛЕЗО CRB-1150", StorageUnit = "шт" },
new Item { ItemName = "ЗМІННЕ ПОЛІУРЕТАНОВЕ ЛЕЗО CRB-1150", StorageUnit = "шт" },
new Item { ItemName = "ПОКОВКА РОЛИКА 31118.654.611 (ЗАМОВЛЕННЯ 0400824)", StorageUnit = "шт" },
new Item { ItemName = "ОБИЧАЙКА 800*1600*12 (КН-00-012В)", StorageUnit = "шт" },
new Item { ItemName = "ОЧИСНИК КОНВЕЄРНОЇ СТРІЧКИ ГС.62-300.00.00АСБ (ЗАМОВЛЕННЯ 0201679)", StorageUnit = "шт" },
new Item { ItemName = "ВІЗОК 47-1076-200СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 1250*1150", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  38-983.02", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ 82.4876.001 (ЗАМОВЛЕННЯ 40279/0405358)", StorageUnit = "шт" },
new Item { ItemName = "ВОРОНКА №1 РОЗВАНТАЖЕННЯ РУДИ НА КОНВЕЄР 62-13650.01.00.00.СБ", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  15.1664.002", StorageUnit = "шт" },
new Item { ItemName = "ПРИСТРІЙ НАТЯЖНИЙ ГВИНТІВИЙ 16063ВУ-100-50", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 500*1400 (89.5010.000СБ)", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  КЦ2-750 (ГС.62-570.00.03)", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПОВІДНИЙ 630*1400 (89-669А.СБ)", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ВІДХИЛЮВАЛЬНИЙ D=1600*2200.ТЗ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ  D=1250*1800.ТЗ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ  D=1250*2200.ТЗ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПОВІДНИЙ 67-660АСБ", StorageUnit = "шт" },
new Item { ItemName = "КОЛІСНА ПАРА ГС.62-667.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 88.1688.000СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 88.1688.000СБ", StorageUnit = "шт" },
new Item { ItemName = "РАМА 3393-01.00.00", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРОВІДНИЙ (D=630) ГС.62-828.00.00.00.СБ", StorageUnit = "шт" },
new Item { ItemName = "СТАКАН 62.12061.00.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  54.3326.001", StorageUnit = "шт" },
new Item { ItemName = "РАМА КОНВЕЄРА 28.4862.00.000Р-01СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 89-527-00А.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 89-527-00А.СБ", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  73.5228.001", StorageUnit = "шт" },
new Item { ItemName = "ДИСК 73.5228.003", StorageUnit = "шт" },
new Item { ItemName = "ДИСК 55.1960.001", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 160-80Г-120.000В.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН КІНЦЕВИЙ 1400.800-100.000БСБ", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  38-417.01", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ D=1600*2200", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ВІДХИЛЯЮЧИЙ D=1250*2200", StorageUnit = "шт" },
new Item { ItemName = "РАМА 61.5123.000СБ", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПРИВІДНИЙ К5-К6 D=160/150/140 L=2723", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПРИВІДНИЙ К7 D=160/150/140 L=2323", StorageUnit = "шт" },
new Item { ItemName = "КОМПЛЕКТ ФІКСУЮЧИХ ЕЛЕМЕНТІВ ПРИВОДНОГО БАРАБАНА LE160", StorageUnit = "компл" },
new Item { ItemName = "КОМПЛЕКТ ФІКСУЮЧИХ ЕЛЕМЕНТІВ ВІДВІДНОГО БАРАБАНА LE80", StorageUnit = "компл" },
new Item { ItemName = "ПІВМУФТА ЕЛЕКТРОДВИГУНА ГС.55-1272.00.01", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ А2.КС.00.001.01", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН КІНЦЕВИЙ d=1250 1400.1600Ф-200-2-000 Б СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПОВІДНИЙ 89-725А.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 53.3197.100А.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 53.3197.100А.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 12065Ф-120 (123.613.000Г.СБ)", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ РСЦ.55.7А.-1687.00.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 26.5065.000.СБ", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА БАРАБАНА РСЦ.55.15-1788.00.01", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПОВІДНИЙ (10043Ф-80) РСЦ.55.89-1785.00.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ВІДХИЛЮВАЛЬНИЙ 160103Ф-200 (89-532 Б СБ)", StorageUnit = "шт" },
new Item { ItemName = "НАТЯЖНИЙ ПРИСТРІЙ 38.512.00.000АСБ", StorageUnit = "шт" },
new Item { ItemName = "ПРОКЛАДКА ГС.55.97-1553.00.01-04", StorageUnit = "шт" },
new Item { ItemName = "БАЛКА ОПОРНА РСЦ.55.25-1852.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ВІДХИЛЯЮЧИЙ 8043Ф-60 38-460.00.000БСБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВIДНИЙ 8053Ф-80 89-700Б.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПОВІДНИЙ 12066Ф-80 89-669Б.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПОВІДНИЙ 10043Ф-60 64.545.000Г.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПОВІДНИЙ 10053Ф-60 89-725Б.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВIДНИЙ 10066Ф-100 89-708В.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВIДНИЙ 10066Ф-100 89-708В.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВIДНИЙ У ЗБОРI 274 34,5Ф-90 28.4877.00.000Р1.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВIДНИЙ У ЗБОРI 274 34,5Ф-90 28.4877.00.000Р1.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ У ЗБОРI 274 34,5Ф-90 28.4874.00.000Р1.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ У ЗБОРI 274 34,5Ф-90 28.4874.00.000Р1.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 12083Ф-80 89.4988.000 А СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН СВАРНОЙ 10034Ф-80 89.726.100Р1 СБ", StorageUnit = "шт" },
new Item { ItemName = "ФУТЕРІВКА LG509 W290 EP10 N14203655", StorageUnit = "шт" },
new Item { ItemName = "ФУТЕРІВКА LG587 L215 EP10 N14203660", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ВІДХИЛЮВАЛЬНИЙ 14043Ф-60 1400.400-60.000ВСБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПОВIДНИЙ 10043Ф-80 РСЦ.55.89-1615.00.00 А СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПОВIДНИЙ 10043Ф-80 РСЦ.55.89-1615.00.00 А СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН 38-487.00 Б СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ВІДХИЛЮВАЛЬНИЙ 38-509.00.000 В СБ", StorageUnit = "шт" },
new Item { ItemName = "ПЛАСТИНА РСЦ.55.97-1834.00.01-04", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ D=325 62.1194.09.04.00.00.СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ 89.1107.000Б.СБ", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПОВІДНИЙ У ЗБОРІ РСЦ.55.73-1964.00.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "ОЧИЩУВАЧ ГРУБОЇ ОЧИСТКИ ROUGH POLIMER CLEANING В=1000", StorageUnit = "шт" },
new Item { ItemName = "ОЧИЩУВАЧ ТОНКОЇ ОЧИСТКИ FINE BE-METAL CLEANING В=1000", StorageUnit = "шт" },
new Item { ItemName = "РОЛИКООПОРА НИЖНЯ САМОРЕГУЛЮЮЧА HOSCH RRC2-1600", StorageUnit = "шт" },
new Item { ItemName = "ХРАПОВИЙ ЗУПИННИК З МУФТОЙ ЗУБЧАТОЙ  СБОРКА ЛІВА 2Бу05470-2", StorageUnit = "шт" },
new Item { ItemName = "РЕШІТКА ПРИЙМАЛЬНОГО БУНКЕРУ РБ 01.000 СБ", StorageUnit = "шт" },
new Item { ItemName = "РАМА ПРИВОДУ ЛК-4 62-12027.10.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "ХРАПОВИЙ ЗУПИННИК З МУФТОЙ ЗУБЧАТОЙ 2Бу05471-1 (ЗБІРКА ПРАВА)", StorageUnit = "шт" },
new Item { ItemName = "РАМА ПРИВІДНОГО БАРАБАНУ ЛК-6 62.12422.00.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "РАМА ПРИВІДНОГО БАРАБАНУ ЛК-7 62.12745.00.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "CКРЕБОК В=1200 ДО ОЧИЩУВАЧА  ПЕРВИННОГО \"FLEXCO\"  MМP660 TRB48/76490", StorageUnit = "шт" },
new Item { ItemName = "НАПІВМУФТА 55-15552.02.00.00.01", StorageUnit = "шт" },
new Item { ItemName = "НАПІВМУФТА 55-15552.02.00.00.01", StorageUnit = "шт" },
new Item { ItemName = "БОРТ ГУМОВИЙ (БЕЗ КОРДА) АМ.62-583.00.01-01", StorageUnit = "шт" },
new Item { ItemName = "РЕМКОМПЛЕКТ ДЛЯ ШТОКА ГІДРОЦИЛІНДРА К505687-900", StorageUnit = "шт" },
new Item { ItemName = "БОРТ ГУМОВИЙ (БЕЗ КОРДА) ДТО.55.15-2054.00.01-01", StorageUnit = "шт" },
new Item { ItemName = "ОЧИЩУВАЛЬНИЙ БЛОК – СУВАЮЧИЙ HOSCH VC1/VC2 (ID 0631010) 200мм", StorageUnit = "шт" },
new Item { ItemName = "РАМА ОПОРНА РУХЛИВА ДТО.55.25-2083.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "ГІДРОЦИЛІНДР HC 150_125_5181.6 В КОМПЛЕКТІ З ДОДАТКОВИМ РЕМОНТНИМ КОМПЛЕКТОМ УЩІЛЬНЕНЬ HC 150_125_5181.6", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ ДТО-2117.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 8040Ф-60 ДТО-2136.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 8040Ф-60 ДТО-2136.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ 10050Ф-80 ДТО-2115.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ 8050Ф-80 ДТО-2118.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 160100Ф-200 ДТО-2123.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ 10050Ф-60 ДТО-2142.01.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 16080Ф-140 ДТО-2213.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 140100Ф-150 ДТО-2138.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 16063Ф-100 ДТО-2128.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 16063Ф-100 ДТО-2128.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 12063Ф-80 ДТО-2122.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 12063Ф-80 ДТО-2122.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 12063Ф-80 ДТО-2122.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 12063Ф-80 ДТО-2122.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "КОМПЛЕКТ МЕТАЛЕВОЇ ФУТЕРІВКИ ДЛЯ ЖОЛОБА РОЗВАНТАЖУВАЛЬНОГО ПТ1002-ТХ.К.8", StorageUnit = "компл" },
new Item { ItemName = "КОМПЛЕКТ МЕТАЛЕВОЇ ФУТЕРІВКИ ДЛЯ ЖОЛОБА РОЗВАНТАЖУВАЛЬНОГО М2659.1.10-ТХ.К.8", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ 12080Ф-160 ПТ 1002-ТХ.К.300", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ 12063Ф-100 М2659.1.10-ТХ.К.300", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ 12080-120 ПТ 1002-ТХ.К.620", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ 12063-100 М2659.1.10-ТХ.К.250", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ОБОРОТНИЙ 12050-80 ПТ 1002-ТХ.К.250", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАННИЙ НІЖ-ОЧИСНИК БН1-04", StorageUnit = "шт" },
new Item { ItemName = "РОЛИКООПОРА ЦЕНТРИРУЮЩАЯ НИЖНЯ НЦГ120-159 ПТ1002-ТХ.К.950", StorageUnit = "шт" },
new Item { ItemName = "РОЛИК КОНВЕЄРНИЙ 159х1400 підш.408 РК 159.408л.00.000-01СБ", StorageUnit = "шт" },
new Item { ItemName = "ЗМІННЕ ЛЕЗО ДО ОЧИСНИКА (ВЕРХНІЙ) QC1HC-48MC40BR+E", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ 10040Ф-100 ДТО-2246.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ 10032Ф-80 ДТО-2245.00.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ 10032Ф-80 ДТО-2245.00.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ 160-125Ф-200 М ДТО-2293.00.00 М ВО", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ 160-125Ф-200 М ДТО-2293.00.00 М ВО", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ 200-160Ф-200 У М ДТО-2295.00.00 М ВО", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ 200-160Ф-200 У М ДТО-2295.00.00 М ВО", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ КОНВЕЄРУ К5-1 4678.1531.35.00.00", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ КОНВЕЄРУ К5-1 4678.1531.36.00.00", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ КОНВЕЄРУ К5-1 4678.1531.37.00.00", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ КОНВЕЄРУ К6-1 4678.1531.38.00.00", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ КОНВЕЄРУ К6-1 4678.1531.39.00.00", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ КОНВЕЄРУ К6-1 4678.1531.40.00.00", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ КОНВЕЄРА ЛК7 46.78-1531-11.02.00", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ КОНВЕЄРА ЛК7 46.78-1531-11.03.00", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ 10043Ф-80 SKF ДТО-2319.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ 10043Ф-80 SKF ДТО-2319.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 160100Ф-200М ДТО-2123.00.00М ВО", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 12063Ф-80М ДТО-2122.00.00М ВО", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕ ПРИВІДНИЙ 12050Ф-100М ДТО-2137.00.00М СБ", StorageUnit = "шт" },
new Item { ItemName = "ПРИВІД ШИБЕРА БЕЗ МОТОР-РЕДУКТОРА ТО-4874.065.000 СБ", StorageUnit = "шт" },
new Item { ItemName = "СТАНЦІЯ НАТЯЖНА 274.32Ф-120 55.23-2564.00.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ГОЛОВНИЙ 14025Ф-120М ДТО.55.25-2244.00.00М1 ВО", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НЕПРИВІДНИЙ 10063Ф-80М ДТО-2706.00.00М ВО", StorageUnit = "шт" },
new Item { ItemName = "ДЕМПФЕРНА СТАНЦІЯ DRXD-48-535", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ 30132", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПРИВІДНИЙ 30133", StorageUnit = "шт" },
new Item { ItemName = "ФІКСОВАНА ОПОРА P4B-IP-090MR", StorageUnit = "шт" },
new Item { ItemName = "ПЛАВАЮЧА ОПОРА P4B-IP-090MRE", StorageUnit = "шт" },
new Item { ItemName = "ШВИДКОХІДНА МУФТА ДО ПРИВОДУ 18,5кВт-EFC-05", StorageUnit = "шт" },
new Item { ItemName = "ШВИДКОХІДНА МУФТА ДО ПРИВОДУ 30кВт-EFC-05", StorageUnit = "шт" },
new Item { ItemName = "РОЛИКООПОРА Б-1400", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ БП 1250.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "РАМА ПРИВІДНОГО БАРАБАНА ГС.62-37.02.00СБ", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН ПОВІДНИЙ 123.613.000АБВсб (ЗАМОВЛЕННЯ 50407/0401633)", StorageUnit = "шт" },
new Item { ItemName = "РОЛИК ЗВОРОТНІЙ У ЗБОРІ РСЦ.55.27-1889.00.00А СБ", StorageUnit = "шт" },
new Item { ItemName = "МУФТА ST-040 З ПІДГОТОВЛЕНИМИ ОТВОРАМИ 55H7/45H7 MARKO ST-040", StorageUnit = "шт" },
new Item { ItemName = "БАРАБАН НАТЯЖНИЙ РСЦ.55.73-1910.01.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "КОЛЕСО ХОДОВЕ 53-922СБ", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  36-1028.01", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ 53.4885.001", StorageUnit = "шт" },
new Item { ItemName = "ПІВМУФТА  53.5239.001", StorageUnit = "шт" },
new Item { ItemName = "ЗІРОЧКА ПОВІДНА 53.2043.001-03", StorageUnit = "шт" },
new Item { ItemName = "ДИСК ПРОМІЖНИЙ 1275.01.322", StorageUnit = "шт" },
new Item { ItemName = "ДИСК ПРОМІЖНИЙ 1275.01.322", StorageUnit = "шт" },
new Item { ItemName = "ДИСК ПРОМІЖНИЙ 1275.01.322", StorageUnit = "шт" },
new Item { ItemName = "ДИСК ПРОМІЖНИЙ 1275.01.322", StorageUnit = "шт" },
new Item { ItemName = "ДИСК ПРОМІЖНИЙ 1275.01.322", StorageUnit = "шт" },
new Item { ItemName = "ЦИЛІНДР ГІДРАВЛІЧНИЙ 1255.08.300-8СБ", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ УЩІЛЬНЮВАЛЬНЕ ВНУТРІШНЄ 442.7108-01", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ СТОПОРНЕ ВЕРХНЄ B963S3004A", StorageUnit = "шт" },
new Item { ItemName = "ПРИВІДНИЙ ВАЛ 1242.06.00-5", StorageUnit = "шт" },
new Item { ItemName = "НАСОС RV6-01V 38-81387 (906.0141-00)", StorageUnit = "шт" },
new Item { ItemName = "КОВПАК ТРАВЕРСИ 442.9164-00", StorageUnit = "шт" },
new Item { ItemName = "ШПОНКА Т28*16*110 857.0336-00", StorageUnit = "шт" },
new Item { ItemName = "ШПОНКА Т28*16*110 857.0336-00", StorageUnit = "шт" },
new Item { ItemName = "ПРОКЛАДКА б=1,5 442.7136-03", StorageUnit = "шт" },
new Item { ItemName = "ГОЛОВНИЙ ВАЛ У ЗБОРІ S-4000 EXP452.1699-902", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ (2134-01 D510/404*20) 442.7169-00", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ (2134-01 D510/404*20) 442.7169-00", StorageUnit = "шт" },
new Item { ItemName = "КОНУС ДРОБАРКИ ККД 1500/180", StorageUnit = "шт" },
new Item { ItemName = "ЕКСЦЕНТРИК 442.8733-01", StorageUnit = "шт" },
new Item { ItemName = "КОЖУХ ЗМІЦНЕНИЙ 62-1071.80.00.000СБ (ЗАМОВЛЕННЯ 0301906)", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ НЕРУХОМОГО КОНУСА (Н-6800) 442.8817-02", StorageUnit = "шт" },
new Item { ItemName = "ЕКСЦЕНТРИК 1255.04.100-4СБ", StorageUnit = "шт" },
new Item { ItemName = "442.7577-01 ПРОБКА", StorageUnit = "шт" },
new Item { ItemName = "ЕКСЦЕНТРИК S4000 442.8068-01", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПРИВОДУ 442.9937-01", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПРИВОДУ 442.9937-01", StorageUnit = "шт" },
new Item { ItemName = "КОЖУХ ВАЛУ-ШЕСТІРНІ 442.7506-01", StorageUnit = "шт" },
new Item { ItemName = "ПРИТИСКНЕ КІЛЬЦЕ 442.8801-01", StorageUnit = "шт" },
new Item { ItemName = "СТОПОРНЕ КІЛЬЦЕ 855.0108-00", StorageUnit = "шт" },
new Item { ItemName = "ЦИЛІНДР HYDROSET 442.7163-01", StorageUnit = "шт" },
new Item { ItemName = "ПЛИТА БОКОВА ПРАВА  2-55730", StorageUnit = "шт" },
new Item { ItemName = "ПЛИТА БОКОВА ЛІВА 2-55729", StorageUnit = "шт" },
new Item { ItemName = "ПІДСТАВКА  ПІД КОНУС ДРОБАРКИ ККД 1500", StorageUnit = "шт" },
new Item { ItemName = "ПОРШЕНЬ 442.9672-00", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ ВЕРХНЄ 1242.04.08", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ 442.7109-01", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ 442.7109-01", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ 442.7109-01", StorageUnit = "шт" },
new Item { ItemName = "САЛЬНИК 950 (1255.08.05)", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПРИВІДНИЙ B963S3002A", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПРИВІДНИЙ B963S3002A", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПРИВІДНИЙ B963S3002A", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА 1255.05.16", StorageUnit = "шт" },
new Item { ItemName = "ВТУЛКА 1255.05.16", StorageUnit = "шт" },
new Item { ItemName = "ПРИВІД ЗІ ШКІВОМ D=1620 1255.07.300-3СБ", StorageUnit = "шт" },
new Item { ItemName = "ПРИВІД ЗІ ШКІВОМ D=695 1255.07.200", StorageUnit = "шт" },
new Item { ItemName = "ШКІВ D=695 1242.07.121", StorageUnit = "шт" },
new Item { ItemName = "ШЕСТЕРНЯ КОНІЧНА m=30, z=26 1255.06.310", StorageUnit = "шт" },
new Item { ItemName = "РЕМКОМПЛЕКТ ЗАРЯДНОГОПОРТУ АЗОТНОЇ КАМЕРИ 94400128", StorageUnit = "шт" },
new Item { ItemName = "ПОРШНЕВЕ КІЛЬЦЕ 1063085321", StorageUnit = "шт" },
new Item { ItemName = "ОЛІЄВІДБИВАЧ 1063587664", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ОПОРНИЙ", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ОПОРНИЙ", StorageUnit = "шт" },
new Item { ItemName = "КАТРИДЖ (ВЕРТИКАЛЬНИЙ ВАЛ У ЗБОРІ) В963S3000A/V", StorageUnit = "шт" },
new Item { ItemName = "КАТРИДЖ (ВЕРТИКАЛЬНИЙ ВАЛ У ЗБОРІ) В963S3000A/V", StorageUnit = "шт" },
new Item { ItemName = "УЩІЛЬНЮВАЛЬНЕ КІЛЬЦЕ 873.0838-00", StorageUnit = "шт" },
new Item { ItemName = "ШЕСТІРНЯ КОНІЧНА Z=21,M=30(кр.1239.02.302-1Б) З.435 КОД 302910", StorageUnit = "шт" },
new Item { ItemName = "ШЕСТІРНЯ КОНІЧНА Z=21,M=30(кр.1239.02.302-1Б) З.435 КОД 302910", StorageUnit = "шт" },
new Item { ItemName = "ФУТЕРІВКА-РЕБРО ВАЛУ(ЗАХИСТ ПРИВОДУ) 442.8760-00", StorageUnit = "шт" },
new Item { ItemName = "ПРИТИСКНЕ КІЛЬЦЕ 442.8425-01", StorageUnit = "шт" },
new Item { ItemName = "КІЛЬЦЕ ФІКСУВАЛЬНЕ 442.7103-01", StorageUnit = "шт" },
new Item { ItemName = "КОРПУС ПІДШИПНИКА 3605-0", StorageUnit = "шт" },
new Item { ItemName = "ШЕСТІРНЯ М10, Z=15 (М608-47/1) (ЗАМОВЛЕННЯ 0400362)", StorageUnit = "шт" },
new Item { ItemName = "КОРПУС (МЕХАНІЗМ ПІДЙОМУ 2 КСН) ГС.62-319.00.00.06", StorageUnit = "шт" },
new Item { ItemName = "КОЛЕСО КОНІЧНЕ ГС62-234.01-Б (ЗАМОВЛЕННЯ 0302249)", StorageUnit = "шт" },
new Item { ItemName = "КОЛЕСО КОНІЧНЕ ГС62-234.01-Б (ЗАМОВЛЕННЯ 0302249)", StorageUnit = "шт" },
new Item { ItemName = "ВІНЕЦЬ ЗУБЧАСТИЙ Z=268, М=20 1-174343-02", StorageUnit = "шт" },
new Item { ItemName = "ВІНЕЦЬ ЗУБЧАСТИЙ Z=268, М=20 1-174343-02", StorageUnit = "шт" },
new Item { ItemName = "ВІНЕЦЬ ЗУБЧАСТИЙ Z=268, М=20 1-174343-02", StorageUnit = "шт" },
new Item { ItemName = "ВІНЕЦЬ ЗУБЧАСТИЙ Z=268, М=20 1-174343-02", StorageUnit = "шт" },
new Item { ItemName = "ВІНЕЦЬ ЗУБЧАСТИЙ Z=268, М=20 1-174343-02", StorageUnit = "шт" },
new Item { ItemName = "ВІНЕЦЬ ЗУБЧАСТИЙ Z=268, М=20 1-174343-02", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПОВІДНИЙ У ЗБОРІ 1-271509-03", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ 69.880.001", StorageUnit = "шт" },
new Item { ItemName = "БУТАРА D=12 (2.3730-05.22ПЧ)", StorageUnit = "шт" },
new Item { ItemName = "ПАТРУБОК РОЗВАНТАЖУВАННЯ 2-145739А", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ-ШЕСТІРНЯ М8, Z=14 (РМ.1000-11-03)", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ-ШЕСТІРНЯ М8, Z=14 (РМ.1000-11-03)", StorageUnit = "шт" },
new Item { ItemName = "НАПІВЦАПФА ОФ-И-522АБ (ЗАМОВЛЕННЯ 0300878)", StorageUnit = "шт" },
new Item { ItemName = "КОРПУС 1-179734 (ЗАМОВЛЕННЯ 0301723усл)", StorageUnit = "шт" },
new Item { ItemName = "ПРОМВАЛ ОФ-И-677СБ (ЗАМОВЛЕННЯ 0301804усл)", StorageUnit = "шт" },
new Item { ItemName = "ПРОМВАЛ ОФ-И-677СБ (ЗАМОВЛЕННЯ 0301804усл)", StorageUnit = "шт" },
new Item { ItemName = "ГОРЛОВИНА ВОРОНКИ 2.66397 (ЗАМОВЛЕННЯ 0300190)", StorageUnit = "шт" },
new Item { ItemName = "ГОРЛОВИНА ВОРОНКИ 2.66397 (ЗАМОВЛЕННЯ 0300190)", StorageUnit = "шт" },
new Item { ItemName = "ГОРЛОВИНА ВОРОНКИ 2.66397 (ЗАМОВЛЕННЯ 0300190)", StorageUnit = "шт" },
new Item { ItemName = "ВОРОНКА РОЗВАНТАЖУВАЛЬНА 62-11297.00.00.00СБ", StorageUnit = "шт" },
new Item { ItemName = "КРИШКА РОЗВАНТАЖУВАЛЬНА 1374.04.101СБ", StorageUnit = "шт" },
new Item { ItemName = "ВАЛ ПОВІДНИЙ У ЗБОРІ 1-230027-11", StorageUnit = "шт" },
new Item { ItemName = "ПЛИТА ОПОРНА 8-66720", StorageUnit = "шт" },
new Item { ItemName = "ПРИВІД МЛИНА 1-180981СБ", StorageUnit = "шт" },
new Item { ItemName = "ШПОНКА 80*45*820 (66.4742.001) ЗАМОВЛЕННЯ 0404691", StorageUnit = "шт" },
new Item { ItemName = "КОРПУС БАРАБАНА 1-224437", StorageUnit = "шт" },
new Item { ItemName = "ПЛИТА ОПОРНА 1-182102", StorageUnit = "шт" },
new Item { ItemName = "ПЛИТА ОПОРНА 1-182102", StorageUnit = "шт" },
new Item { ItemName = "НАПІВЦАПФА ГС.62-527.00.01", StorageUnit = "шт" },
new Item { ItemName = "РОЛИКООПОРА СМ6001.02.00.000СБ", StorageUnit = "шт" },
new Item { ItemName = "ПАТРУБОК РОЗВАНТАЖУВАЛЬНИЙ МСЦ 3,6*5,5 ГС.62-785.00.00 СБ", StorageUnit = "шт" },
new Item { ItemName = "ЕЛЕВАТОР МШР 4,43*5,01 ГС.62-220.00.01-В", StorageUnit = "шт" },
new Item { ItemName = "ПЛИТА ФУНДАМЕНТНА 1-156915", StorageUnit = "шт" },
new Item { ItemName = "СКОБА КОНТРОЛЮ ДІАМЕТРІВ 1350 ГС.62-938.01.00.СБ", StorageUnit = "шт" },
new Item { ItemName = "КРИШКА ПІДШИПНИКА 3Б.1537-3І1", StorageUnit = "шт" },
new Item { ItemName = "БУТАРА ДО СТРИЖНЕВОГО МЛИНА ГС.62-1094.00.00-А СБ", StorageUnit = "шт" },
new Item { ItemName = "БУТАРА ДО СТРИЖНЕВОГО МЛИНА ГС.62-1094.00.00-А СБ", StorageUnit = "шт" },
new Item { ItemName = "НАПІВБУТАРА D=14 ГС.62-736.00.00.СБ", StorageUnit = "шт" },
new Item { ItemName = "СТІНКА ЗАВАНТАЖУВАЛЬНА МШР 4,0*5,0 1-224142-01", StorageUnit = "шт" },
new Item { ItemName = "ВОРОНКА РОЗВАНТАЖУВАЛЬНА ПРАВА 1374.04.102", StorageUnit = "шт" },
new Item { ItemName = "ПАТРУБОК ЗАВАНТАЖУВАЛЬНИЙ ОФ2-МШ-5К", StorageUnit = "шт" },
new Item { ItemName = "ПЛИТА ФУНДАМЕНТНА ПРИВІДНОГО ВАЛУ МСЦ 3600*5500 (8-4038)", StorageUnit = "шт" },
new Item { ItemName = "СТІНКА ТОРЦЕВА 1-145079СБ", StorageUnit = "шт" },

                };

                db.Items.AddRange(items);
                await db.SaveChangesAsync();
                Console.WriteLine($"Seeded {items.Count} items into DB from the test list.");
            }
            else
            {
                Console.WriteLine("Items already exist in DB. Skipping seeding items.");
            }
        }
    }
}
