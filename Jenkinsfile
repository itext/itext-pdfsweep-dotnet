#!/usr/bin/env groovy
@Library('pipeline-library') _

def schedule
switch (env.BRANCH_NAME) {
    case ~/.*master.*/:
        schedule = '@monthly'
        break
    case ~/.*develop.*/:
        schedule = '@midnight'
        break
    default:
        schedule = ''
        break
}

pipeline {

    agent { label 'windows' }

    options {
        ansiColor 'xterm'
        buildDiscarder logRotator(artifactNumToKeepStr: '1')
        parallelsAlwaysFailFast()
        skipStagesAfterUnstable()
        timeout time: 1, unit: 'HOURS'
        timestamps()
    }

    triggers {
        cron(schedule)
    }

    stages {
        stage('Wait for blocking jobs') {
            steps {
                script {
                    properties[[
                            $class         : 'BuildBlockerProperty',
                            blockLevel     : 'GLOBAL',
                            blockingJobs   : ".*NET/itextcore/$env.JOB_BASE_NAME\$",
                            scanQueueFor   : 'ALL',
                            useBuildBlocker: true
                    ]]
                }
            }
        }
        stage('Build') {
            options {
                retry 2
            }
            stages {
                stage('Clean workspace') {
                    options {
                        timeout time: 5, unit: 'MINUTES'
                    }
                    steps {
                        cleanWs deleteDirs: true, patterns: [
                                [pattern: 'packages', type: 'INCLUDE'],
                                [pattern: 'global-packages', type: 'INCLUDE'],
                                [pattern: 'tmp/NuGetScratch', type: 'INCLUDE'],
                                [pattern: 'http-cache', type: 'INCLUDE'],
                                [pattern: 'plugins-cache', type: 'INCLUDE'],
                                [pattern: '**/obj', type: 'INCLUDE'],
                                [pattern: '**/bin', type: 'INCLUDE'],
                                [pattern: '**/*.nupkg', type: 'INCLUDE']
                        ]
                    }
                }
                stage('Install branch dependencies') {
                    options {
                        timeout time: 5, unit: 'MINUTES'
                    }
                    when {
                        not {
                            anyOf {
                                branch "master"
                                branch "develop"
                            }
                        }
                    }
                    steps {
                        script {
                            installBranchArtifactsDotnet "branch-artifacts/$env.JOB_BASE_NAME/**/dotnet/", "$env.WORKSPACE"
                        }
                    }
                }
                stage('Compile') {
                    options {
                        timeout time: 20, unit: 'MINUTES'
                    }
                    steps {
                        withEnv([
                                "temp=$env.WORKSPACE/tmp/NuGetScratch",
                                "NUGET_PACKAGES=$env.WORKSPACE/global-packages",
                                "NUGET_HTTP_CACHE_PATH=$env.WORKSPACE/http-cache",
                                "NUGET_PLUGINS_CACHE_PATH=$env.WORKSPACE/plugins-cache",
                                "gsExec=$gsExec", "compareExec=$compareExec"
                        ]) {
                            bat "\"$env.NuGet\" restore itext.cleanup.sln"
                            bat "dotnet restore itext.cleanup.sln"
                            bat "dotnet build itext.cleanup.sln --configuration Release --source $env.WORKSPACE/packages"
                            script {
                                createPackAllFile findFiles(glob: '**/*.nuspec')
                                load 'packAll.groovy'
                            }
                        }
                    }
                }
            }
            post {
                failure {
                    sleep time: 2, unit: 'MINUTES'
                }
                success {
                    script { currentBuild.result = 'SUCCESS' }
                }
            }
        }
        stage('Run Tests') {
            options {
                timeout time: 1, unit: 'HOURS'
            }
            steps {
                withEnv([
                        "temp=$env.WORKSPACE/tmp/NuGetScratch",
                        "NUGET_PACKAGES=$env.WORKSPACE/global-packages",
                        "NUGET_HTTP_CACHE_PATH=$env.WORKSPACE/http-cache",
                        "NUGET_PLUGINS_CACHE_PATH=$env.WORKSPACE/plugins-cache",
                        "gsExec=$gsExec", "compareExec=$compareExec"
                ]) {
                    script {
                        createRunTestDllsFile findFiles(glob: '**/itext.*.tests.dll')
                        load 'runTestDlls.groovy'
                        createRunTestCsProjsFile findFiles(glob: '**/itext.*.tests.netstandard.csproj')
                        load 'runTestCsProjs.groovy'
                    }
                }
            }
        }
        stage('Artifactory Deploy') {
            options {
                timeout time: 5, unit: 'MINUTES'
            }
            when {
                anyOf {
                    branch "master"
                    branch "develop"
                }
            }
            steps {
                script {
                    getAndConfigureJFrogCLI()
                    findFiles(glob: '*.nupkg').each { item -> upload item }
                }
            }
        }
        stage('Branch Artifactory Deploy') {
            options {
                timeout time: 5, unit: 'MINUTES'
            }
            when {
                not {
                    anyOf {
                        branch "master"
                        branch "develop"
                    }
                }
            }
            steps {
                script {
                    if (env.GIT_URL) {
                        repoName = ("$env.GIT_URL" =~ /(.*\/)(.*)(\.git)/)[0][2]
                        findFiles(glob: '*.nupkg').each { item ->
                            sh "./jfrog rt u \"$item.path\" branch-artifacts/$env.BRANCH_NAME/$repoName/dotnet/ --recursive=false --build-name $env.BRANCH_NAME --build-number $env.BUILD_NUMBER --props \"vcs.revision=$env.GIT_COMMIT;repo.name=$repoName\""
                        }
                    }
                }
            }
        }
        stage('Archive Artifacts') {
            options {
                timeout time: 5, unit: 'MINUTES'
            }
            steps {
                archiveArtifacts allowEmptyArchive: true, artifacts: '*.nupkg'
            }
        }
    }

    post {
        always {
            echo 'One way or another, I have finished \uD83E\uDD16'
        }
        success {
            echo 'I succeeeded! \u263A'
            cleanWs deleteDirs: true
        }
        unstable {
            echo 'I am unstable \uD83D\uDE2E'
        }
        failure {
            echo 'I failed \uD83D\uDCA9'
        }
        changed {
            echo 'Things were different before... \uD83E\uDD14'
        }
        fixed {
            script {
                if (env.BRANCH_NAME.contains('master') || env.BRANCH_NAME.contains('develop')) {
                    slackNotifier "#ci", currentBuild.currentResult, "$env.BRANCH_NAME - Back to normal"
                }
            }
        }
        regression {
            script {
                if (env.BRANCH_NAME.contains('master') || env.BRANCH_NAME.contains('develop')) {
                    slackNotifier "#ci", currentBuild.currentResult, "$env.BRANCH_NAME - First failure"
                }
            }
        }
    }

}

def upload(item) {
    def dir = (item =~ /(.*?)(\.[0-9]*\.[0-9]*\.[0-9]*(-SNAPSHOT)?\.nupkg)/)[0][1]
    sh "./jfrog rt u \"$item.path\" nuget/$dir/ --flat=false --build-name $env.BRANCH_NAME --build-number $env.BUILD_NUMBER"
}

