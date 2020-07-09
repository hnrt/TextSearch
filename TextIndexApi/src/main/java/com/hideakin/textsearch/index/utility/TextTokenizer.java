package com.hideakin.textsearch.index.utility;

import java.io.BufferedReader;
import java.io.ByteArrayInputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.Reader;
import java.io.StringReader;
import java.nio.charset.Charset;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import com.hideakin.textsearch.index.model.TextToken;

public class TextTokenizer {

	public static final int MAX_LEN = 255;
	
	public static final String NEWLINE = "\n";
	
	private static final int EOS = -1;
	
	private List<TextToken> tokens;
	
	private StringBuilder buf;
	
	private int row;
	
	private int col;
	
	private Reader input;
	
	private int c;
	
	private int tRow;
	
	private int tCol;
	
	public TextTokenizer() {
		tokens = new ArrayList<TextToken>();
		buf = new StringBuilder();
		row = 0;
		col = 0;
	}

	public Map<String, List<Integer>> populateTextMap() {
		Map<String, List<Integer>> map = new HashMap<String,List<Integer>>();
		for (int index = 0; index < tokens.size(); index++) {
			String text = tokens.get(index).getText();
			List<Integer> positions = map.get(text);
			if (positions == null) {
				positions = new ArrayList<Integer>();
				map.put(text, positions);
			}
			positions.add(index);
		}
		return map;
	}
	
	public String[] getTexts() {
		String[] texts = new String[tokens.size()];
		for (int index = 0; index < texts.length; index++) {
			texts[index] = tokens.get(index).getText();
		}
		return texts;
	}
	
	public void run(byte[] data, String charset) {
		try (Reader input = new BufferedReader(new InputStreamReader(new ByteArrayInputStream(data), Charset.forName(charset)))) {
			run(input);
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	public void run(String data) {
		try (Reader input = new StringReader(data)) {
			run(input);
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	public void run(Reader input) {
		try {
			initialize(input);
			while (c != EOS) {
				if (Character.isWhitespace(c)) {
					read();
				} else if (UnicodeClassifier.isJapaneseLetter(c)) {
					start();
					read();
					if (UnicodeClassifier.isJapaneseLetter(c)) {
						append();
					}
					endAsIs();
				} else if (UnicodeClassifier.isFullwidthAlphaNumeric(c)) {
					start();
					read();
					while (buf.length() < MAX_LEN && UnicodeClassifier.isFullwidthAlphaNumeric(c)) {
						append();
						read();
					}
					endAsIs();
				} else if (Character.isLetterOrDigit(c) || c == '_') {
					start();
					read();
					while (buf.length() < MAX_LEN && (Character.isLetterOrDigit(c) || c == '_')) {
						append();
						read();
					}
					endAsUpper();
				} else {
					read();
				}
			}
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

	private void initialize(Reader input) throws IOException {
		this.input = input;
		this.c = input.read();
	}

	private void read() throws IOException {
		if (c == '\n') {
			row++;
			col = 0;
		} else {
			col++;
		}
		c = input.read();
	}

	private void start() {
		tRow = row;
		tCol = col;
		buf.setLength(0);
		buf.append((char)c);
	}

	private void append() {
		buf.append((char)c);
	}

	private void endAsIs() {
		tokens.add(new TextToken(buf.toString(), tRow, tCol));
	}

	private void endAsUpper() {
		tokens.add(new TextToken(buf.toString().toUpperCase(), tRow, tCol));
	}

}
