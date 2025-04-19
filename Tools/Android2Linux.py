import os

def process_files(input_folder, output_folder):
    # 定义目标字节序列 A 和替换规则
    target_sequence = bytes([0x32, 0x30, 0x32, 0x32, 0x2E, 0x33, 0x2E, 0x31, 0x35, 0x66, 0x31])
    replacement_byte = 0x18

    # 创建 log 文件
    log_file_path = os.path.join(output_folder, "log.txt")
    with open(log_file_path, "w") as log_file:
        log_file.write("Files without target sequence or insufficient length:\n")

    # 遍历输入文件夹中的所有文件
    for root, _, files in os.walk(input_folder):
        for file in files:
            input_file_path = os.path.join(root, file)
            relative_path = os.path.relpath(input_file_path, input_folder)
            output_file_path = os.path.join(output_folder, relative_path)

            # 确保输出文件夹存在
            os.makedirs(os.path.dirname(output_file_path), exist_ok=True)

            try:
                # 读取文件内容
                with open(input_file_path, "rb") as f:
                    content = f.read()

                # 查找第一次和第二次出现的序列 A 的位置
                first_index = content.find(target_sequence)
                if first_index != -1:
                    second_index = content.find(target_sequence, first_index + len(target_sequence))
                else:
                    second_index = -1

                if second_index != -1:
                    # 计算需要修改的位置（第二次序列 A 后两个字节的第二个字节）
                    modify_position = second_index + len(target_sequence) + 1  # 跳过第一个字节

                    # 检查是否有足够的长度进行修改
                    if modify_position < len(content):
                        modified_content = bytearray(content)
                        modified_content[modify_position] = replacement_byte  # 替换第二个字节

                        # 写入修改后的内容到输出文件
                        with open(output_file_path, "wb") as f:
                            f.write(modified_content)
                    else:
                        # 如果文件长度不足，记录到日志中
                        with open(log_file_path, "a") as log_file:
                            log_file.write(f"{input_file_path} (insufficient length after second sequence)\n")
                else:
                    # 如果未找到两次序列 A，记录到日志中
                    with open(log_file_path, "a") as log_file:
                        log_file.write(f"{input_file_path}\n")

            except Exception as e:
                print(f"Error processing file: {input_file_path} ({e})")

    print("Processing complete. Log of files without the target sequence written to:", log_file_path)


if __name__ == "__main__":
    # 设置输入和输出文件夹路径
    input_folder = input("请输入输入文件夹路径: ").strip()
    output_folder = input("请输入输出文件夹路径: ").strip()

    # 检查输入文件夹是否存在
    if not os.path.isdir(input_folder):
        print(f"输入文件夹不存在: {input_folder}")
    else:
        # 创建输出文件夹（如果不存在）
        os.makedirs(output_folder, exist_ok=True)

        # 开始处理文件
        process_files(input_folder, output_folder)
