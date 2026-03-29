# .\Kill-Port -Port 5046 -Force

function Kill-Port {
    param(
        [Parameter(Mandatory = $true)]
        [int]$Port,

        [switch]$Force
    )

    Write-Host "Verificando porta $Port..."

    $connections = Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue |
        Where-Object { $_.OwningProcess -ne 0 }

    if (-not $connections) {
        Write-Host "Nenhum processo ativo encontrado na porta $Port."
        return
    }

    $pids = $connections | Select-Object -ExpandProperty OwningProcess -Unique

    foreach ($pid in $pids) {
        try {
            $process = Get-Process -Id $pid -ErrorAction Stop

            Write-Host "Porta $Port -> PID $pid ($($process.ProcessName))"

            if (-not $Force) {
                $confirm = Read-Host "Deseja encerrar esse processo? (y/n)"
                if ($confirm -ne "y") {
                    continue
                }
            }

            Stop-Process -Id $pid -Force -ErrorAction Stop
            Start-Sleep -Milliseconds 300

            if (Get-Process -Id $pid -ErrorAction SilentlyContinue) {
                Write-Host "PID ${pid} ainda ativo. Tentando taskkill..."

                cmd /c "taskkill /PID $pid /F" | Out-Null
            }
            else {
                Write-Host "PID ${pid} finalizado."
            }
        }
        catch {
            Write-Host "Erro ao processar PID ${pid}: $($_)"
        }
    }

    Write-Host "Concluído."
}
