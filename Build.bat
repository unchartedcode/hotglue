@echo off

gem which bundler 2>&1|findstr /r /c:"^ERROR" >nul && (
  gem install bundler
) || (
  echo Bundler is installed
)

cmd /c bundle install
rake