import csv

input_file = 'translations.csv'
languages = ['zh-CN', 'en-US', 'es-ES', 'ja-JP', 'ko-KR', 'zh-TW']

with open(input_file, newline='', encoding='utf-8-sig') as csvfile:
    reader = csv.DictReader(csvfile)
    
    # 遍历CSV文件中的每一行
    for row in reader:
        values_list = list(row.values())
        is_comment_row = all(value == '' for value in values_list[1:]) and row['zh-CN'] != ''

        for target_language in languages:
            output_file = f'translation_{target_language}.conf'

            with open(output_file, 'a', encoding='utf-8') as out:
                source_text = row['zh-CN']
                target_text = row.get(target_language, '')

                if is_comment_row:
                    out.write(f'{source_text}\n')
                elif not any(row.values()):
                    out.write('\n') 
                elif source_text and not target_text:
                    out.write(f'{source_text}->{source_text}\n')
                else:
                    out.write(f'{source_text}->{target_text}\n')
