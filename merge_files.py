import os

def gather_files(root_folder, extensions, exclude_dirs, exclude_files, exclude_patterns):
    output_file = "AllProjectFiles_Cleaned.txt"
    files_added = []
    with open(output_file, "w", encoding="utf-8") as out:
        for subdir, dirs, files in os.walk(root_folder):
            # Логування перевірки директорій

            # Виключаємо непотрібні каталоги
            if any(excluded in subdir for excluded in exclude_dirs):
                continue

            for file in files:
                file_lower = file.lower()
                file_path = os.path.join(subdir, file)

                # Логування перевірки файлу

                # Виключаємо конкретні файли
                if file_path in exclude_files:
                    continue

                # Виключаємо файли за патернами (але дозволяємо app.css!)
                if any(file_lower.endswith(pattern) for pattern in exclude_patterns) and "app.css" not in file_lower:
                    continue

                if any(file_lower.endswith(ext) for ext in extensions):
                    try:
                        with open(file_path, "r", encoding="utf-8") as f:
                            out.write(f"--- File: {file} (Path: {file_path}) ---\n\n")
                            out.write(f.read() + "\n\n")
                            files_added.append(file_path)
                    except Exception as e:
                        print(f"⚠️ Помилка читання {file}: {e}")

    print(f"\n✅ Зібрано {len(files_added)} файлів у '{output_file}'")
    for path in files_added:
        print(f"📄 Додано: {path}")

if __name__ == "__main__":
    needed_extensions = [
        ".cs", ".razor", ".cshtml", ".json", ".html", ".xml", ".js", ".css",
        ".ts", ".yml", ".yaml", ".sh", ".ps1", ".md", ".conf", ""
    ]

    # Додаємо "Migrations" до списку папок, які ігноруються
    exclude_dirs = [
        "bin", "obj", "wwwroot/_framework", "scopedcss", "node_modules", ".vs", ".git", "Migrations"
    ]

    exclude_files = [
        ".git/ms-persist.xml",
        ".vs/Prymara/v17/DocumentLayout.backup.json",
        ".vs/Prymara/v17/DocumentLayout.json",
        ".vs/Prymara.API/v17/DocumentLayout.backup.json",
        ".vs/Prymara.API/v17/DocumentLayout.json",
        "Prymara.Client/wwwroot/css/bootstrap/bootstrap.min.css"
    ]

    exclude_patterns = [
        ".min.css",  # Виключає всі мінімізовані CSS файли (крім app.css)
        ".min.js", ".txt", ".dockerignore", ".gitattributes", ".gitignore", ".sln", ".user", ".csproj", ".map", ".py"
    ]

    gather_files(".", needed_extensions, exclude_dirs, exclude_files, exclude_patterns)

    # Затримка перед закриттям консолі (за потреби)
    input("Натисніть Enter, щоб вийти...")
