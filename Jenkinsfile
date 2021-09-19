#!/usr/bin/env groovy
@Library('pipeline-library@DEVSIX-5706')_

def repoName = "pdfSweep"
def dependencyRegex = "itextcore"
def solutionFile = "itext.cleanup.sln"
def frameworksToTest = "net461"

automaticDotnetBuild(repoName, dependencyRegex, solutionFile, frameworksToTest)