from pathlib import Path
import oead
import os
from bcml import util

def main():
    path = Path(os.getcwd()).glob('**/*.smubin')
    files = [x for x in path if x.is_file()]

    for file in files:

        file = Path(file)
        data = file.read_bytes()

        # Parse mod smubin file
        if file.suffix.startswith('.s'):
            data = oead.yaz0.decompress(data)
        mod_byml = oead.byml.from_binary(data)

        # Parse corresponding vanilla smubin file
        game_file = Path(os.path.join(util.get_aoc_dir(), f'Map\\MainField\\{file.name.split("_")[0]}\\{file.name}'))
        game_file = oead.yaz0.decompress(game_file.read_bytes())
        game_byml = oead.byml.from_binary(game_file)

        diff = list()

        for item in mod_byml['Objs']:
            if item not in game_byml['Objs']:
                diff.append(item)
        
        for item in diff:
            print(f"Found: {item['UnitConfigName']}")

            settings = Path(os.environ['LOCALAPPDATA'] + '\\BotwData\\cactors.txt')
            if Path(f"{settings.read_text()}\\Actor\\Pack\\{item['UnitConfigName']}C.sbactorpack").is_file() and not str(item['UnitConfigName']).endswith('C'):
                for obj in mod_byml['Objs']:
                    if obj['HashId'] is item['HashId']:
                        print(f"Updated: {item['UnitConfigName']}C")
                        obj['UnitConfigName'] = f"{item['UnitConfigName']}C"

        data = oead.byml.to_binary(mod_byml, big_endian=True)
        data = oead.yaz0.compress(data)
        Path(file).write_bytes(data)

if __name__ == "__main__":
    main()