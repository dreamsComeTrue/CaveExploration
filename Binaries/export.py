import shutil
import os
import glob

files_to_save = ['GodotSharp.dll',
                 'mscorlib.dll',
                 'netstandard.dll',
                 'System.Core.dll',
                 'System.dll',
                 'unHIDDEN.dll']

for file_to_remove in glob.glob('./data_unHIDDEN/Assemblies/*.*'):
    if not os.path.basename(file_to_remove) in files_to_save:
        os.remove(file_to_remove)

shutil.rmtree('./data_unHIDDEN/Mono', ignore_errors=True)
