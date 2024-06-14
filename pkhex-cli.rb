class PkhexCli < Formula
  version "v0.0.14"
  desc "A CLI to manipulate pokemon game save files."
  homepage "https://github.com/arleypadua/PKHeX.CLI"

  if OS.mac? && Hardware::CPU.arm?
    url "https://github.com/arleypadua/PKHeX.CLI/releases/download/#{version}/pkhex-cli-osx-arm64.zip"
    sha256 "20205dc4affe4e20abcc87b38287d2df972c9d2825680959e11546f8d2e60e1b"
  elsif OS.mac?
    url "https://github.com/arleypadua/PKHeX.CLI/releases/download/#{version}/pkhex-cli-osx-x64.zip"
    sha256 "ef859986936a04fbd2bb9aacf0d4f7a0d5f36e9def99a92194e53c3701f79e60"
  elsif OS.linux?
    url "https://github.com/arleypadua/PKHeX.CLI/releases/download/#{version}/pkhex-cli-linux-x64.zip"
    sha256 "9bdf6584d0420e58282acec0eedbcf489e756abc89a5f484f4acf4c684051eb8"
  end

  def install
    bin.install "pkhex-cli"
  end

  test do
    system "#{bin}/pkhex-cli"
  end
end