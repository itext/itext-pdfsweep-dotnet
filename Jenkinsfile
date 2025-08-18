#!/usr/bin/env groovy
@Library('pipeline-library')_

def repoName = "pdfSweep"
def dependencyRegex = "itextcore"
def solutionFile = "itext.cleanup.sln"
def frameworksToTest = "net461"

withEnv(
    ['ITEXT_VERAPDFVALIDATOR_ENABLE_SERVER=true', 
     'ITEXT_VERAPDFVALIDATOR_PORT=8091']) {
    automaticDotnetBuild(repoName, dependencyRegex, solutionFile, frameworksToTest)
}
