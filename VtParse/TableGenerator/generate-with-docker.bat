docker run -it --rm --name VtParser.Generator -v "%CD%\..":/app -w /app ruby:2.5 ruby ./TableGenerator/vtparse_gen_c_tables.rb ""

pause