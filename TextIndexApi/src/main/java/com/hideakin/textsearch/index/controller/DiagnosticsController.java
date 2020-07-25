package com.hideakin.textsearch.index.controller;

import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestMethod;
import org.springframework.web.bind.annotation.RestController;

import com.hideakin.textsearch.index.model.IdStatus;
import com.hideakin.textsearch.index.model.IndexStats;
import com.hideakin.textsearch.index.service.DiagnosticsService;

@RestController
public class DiagnosticsController {

	private static final Logger logger = LoggerFactory.getLogger(DiagnosticsController.class);

	@Autowired
	private DiagnosticsService diagService;
	
	@RequestMapping(value="/v1/diagnostics/ids",method=RequestMethod.GET)
	public ResponseEntity<?> getIds() {
		return new ResponseEntity<>(diagService.getIds(), HttpStatus.OK);
	}

	@RequestMapping(value="/v1/diagnostics/ids/unused",method=RequestMethod.DELETE)
	public ResponseEntity<?> resetIds() {
		IdStatus prev = diagService.getIds();
		logger.info(String.format("Previously UID.next=%d GID.next=%d FID.next=%d", prev.getUid(), prev.getGid(), prev.getFid()));
		IdStatus next = diagService.resetIds();
		logger.info(String.format("Currently UID.next=%d GID.next=%d FID.next=%d", next.getUid(), next.getGid(), next.getFid()));
		return new ResponseEntity<>(next, HttpStatus.OK);
	}

	@RequestMapping(value="/v1/diagnostics/files/unused",method=RequestMethod.GET)
	public ResponseEntity<?> findUnusedFiles() {
		return new ResponseEntity<>(diagService.findUnusedFiles(), HttpStatus.OK);
	}

	@RequestMapping(value="/v1/diagnostics/files/unused",method=RequestMethod.DELETE)
	public ResponseEntity<?> deleteUnusedFiles() {
		return new ResponseEntity<>(diagService.deleteUnusedFiles(), HttpStatus.OK);
	}

	@RequestMapping(value="/v1/diagnostics/contents/unused",method=RequestMethod.GET)
	public ResponseEntity<?> findUnusedContents() {
		return new ResponseEntity<>(diagService.findUnusedContents(), HttpStatus.OK);
	}

	@RequestMapping(value="/v1/diagnostics/contents/unused",method=RequestMethod.DELETE)
	public ResponseEntity<?> deleteUnusedContents() {
		return new ResponseEntity<>(diagService.deleteUnusedContents(), HttpStatus.OK);
	}

	@RequestMapping(value="/v1/diagnostics/index/{group:[^0-9].*}/stats",method=RequestMethod.GET)
	public ResponseEntity<?> getIndexStats(
			@PathVariable String group) {
		IndexStats status = diagService.getIndexStats(group);
		if (status != null) {
			return new ResponseEntity<>(status, HttpStatus.OK);
		} else {
			return new ResponseEntity<>(HttpStatus.NOT_FOUND);
		}
	}

}
